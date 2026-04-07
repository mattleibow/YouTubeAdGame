using YouTubeAdGame.Engine.Core;
using YouTubeAdGame.Engine.Maps;
using YouTubeAdGame.Engine.Objects;
using YouTubeAdGame.Engine.Effects;

namespace YouTubeAdGame.Engine.Engine;

/// <summary>
/// Central game engine: fixed-timestep update loop, spawn system, collision detection.
/// Platform-independent — no rendering or input code here.
/// </summary>
public sealed class GameEngine(IInputProvider input)
{
    private readonly SpawnSystem _spawn = new();
    private static readonly System.Random Rng = System.Random.Shared;

    // ── Public API ──────────────────────────────────────────────────────────

    /// <summary>Start a new game session.</summary>
    public void StartGame(GameState state)
    {
        state.Phase = GamePhase.Playing;
        state.Score = 0;
        state.Wave  = 1;
        state.Distance = 0;
        state.ScrollOffset = 0;
        state.GameTime = 0;
        state.Player.WorldX = 0;
        state.Player.Health = 3;
        state.Player.GunLevel = 0;
        state.Player.VelocityX = 0;
        state.Player.HitFlashTimer = 0;
        state.Crowd.Count = 1;
        state.Enemies.Clear();
        state.PlayerBullets.Clear();
        state.EnemyBullets.Clear();
        state.Gates.Clear();
        state.Obstacles.Clear();
        state.PowerUps.Clear();
        state.Barriers.Clear();
        state.FloatingTexts.Clear();
        state.Particles.Clear();
        state.ActiveEffects.Clear();
        state.EnemySpawnTimer = 0;
        state.GateSpawnTimer  = 0.5f;  // first gate row arrives quickly
        state.PlayerFireTimer = 0;
        state.PowerUpSpawnTimer = 3f;   // first power-up after a short delay
        state.BarrierSpawnTimer = 15f;  // first barrier after 15 seconds

        // Load the map definition for the selected mode.
        // For Custom mode, preserve the caller-supplied ActiveMap (if any).
        if (state.Mode != GameMode.Custom || state.ActiveMap is null)
            state.ActiveMap = MapRegistry.Get(state.Mode);

        // Initialise runtime-adjustable state from the map definition.
        var map = state.ActiveMap;
        state.MaxEnemiesOnScreen = map.MaxEnemies;
        state.SpawnInterval      = map.SpawnInterval;

        _spawn.SpawnInitialHorde(state);
    }

    /// <summary>
    /// Advance the simulation by <paramref name="dt"/> seconds.
    /// Call on every fixed-timestep tick (60 Hz).
    /// </summary>
    public void Update(GameState state, float dt)
    {
        if (state.Phase != GamePhase.Playing) return;

        state.GameTime += dt;

        var inputState = input.Poll();

        UpdatePlayer(state, inputState, dt);
        UpdateBullets(state, dt);
        UpdateEnemies(state, dt);
        UpdateGateMovement(state, dt);
        UpdatePowerUps(state, dt);
        UpdateBarriers(state, dt);
        UpdateActiveEffects(state, dt);
        UpdateEffects(state, dt);
        UpdateSpawn(state, dt);
        CheckCollisions(state);
        PruneDestroyed(state);
        CheckEndConditions(state);
    }

    // ── Private update steps ────────────────────────────────────────────────

    private static void UpdatePlayer(GameState state, Core.InputState inputState, float dt)
    {
        var player = state.Player;

        // Horizontal movement — acceleration + drag tuned for ~PlayerSpeed terminal velocity
        const float accel = 2000f;
        const float drag  = 0.95f;

        // Speed boost from active effect
        float speedMul = HasActiveEffect(state, PowerUpType.SpeedBoost) ? 1.5f : 1f;

        player.VelocityX += inputState.HorizontalAxis * accel * dt;
        player.VelocityX *= drag;

        // Move and clamp to road bounds — keep the full crowd formation on-road
        player.WorldX += player.VelocityX * dt * speedMul;
        player.WorldX = System.Math.Clamp(player.WorldX,
            -GameConstants.WorldHalfWidth + GameConstants.CrowdHalfWidth,
             GameConstants.WorldHalfWidth - GameConstants.CrowdHalfWidth);

        // Auto-fire
        player.HitFlashTimer = System.Math.Max(0f, player.HitFlashTimer - dt);

        float fireRate = HasActiveEffect(state, PowerUpType.RapidFire)
            ? GameConstants.PlayerFireRate * 0.4f
            : GameConstants.PlayerFireRate;

        state.PlayerFireTimer -= dt;
        if (state.PlayerFireTimer <= 0f)
        {
            FirePlayerBullets(state);
            state.PlayerFireTimer = fireRate;
        }

        // Scroll world forward
        state.ScrollOffset += GameConstants.EnemySpeed * 0.6f * dt;
        state.Distance += GameConstants.EnemySpeed * 0.6f * dt;
    }

    private static void FirePlayerBullets(GameState state)
    {
        // Collect the positions of all visible soldiers
        var positions = state.Crowd
            .GetMemberPositions(state.Player.WorldX, state.Player.Depth, state.GameTime)
            .ToList();
        if (positions.Count == 0) return;

        // Collect the closest enemies (lowest depth = nearest player) as targets
        // Pick the top N closest, from which each shooter picks randomly
        const int targetPoolSize = 10;
        var targets = state.Enemies
            .Where(e => !e.IsDestroyed)
            .OrderBy(e => e.Depth)
            .Take(targetPoolSize)
            .ToList();

        // How many soldiers fire this salvo?
        int maxShooters = System.Math.Clamp(state.MaxConcurrentShooters, 1, positions.Count);
        int shotsPerSoldier = 1 + state.Player.GunLevel;

        // Shuffle positions so we pick a random subset
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int j = Rng.Next(i + 1);
            (positions[i], positions[j]) = (positions[j], positions[i]);
        }

        float bulletSpeed = state.ActiveMap?.BulletSpeed ?? GameConstants.BulletSpeed;
        int totalBullets  = 0;

        for (int s = 0; s < maxShooters && s < positions.Count; s++)
        {
            var (wx, depth) = positions[s];

            for (int burst = 0; burst < shotsPerSoldier; burst++)
            {
                if (totalBullets >= 200) goto Done; // hard cap

                float vx = 0f;
                float speed = bulletSpeed;

                if (targets.Count > 0)
                {
                    // Aim at a random zombie from the target pool
                    var target = targets[Rng.Next(targets.Count)];
                    float dx = target.WorldX - wx;
                    float dz = target.Depth  - depth;
                    float dist = System.MathF.Sqrt(dx * dx + dz * dz);
                    if (dist > 0.001f)
                    {
                        vx    = (dx / dist) * bulletSpeed;
                        speed = (dz / dist) * bulletSpeed;
                        if (speed < 50f) speed = 50f; // ensure bullets always move forward
                    }
                }

                state.PlayerBullets.Add(new Bullet(BulletOwner.Player, wx, depth, speed, vx));
                totalBullets++;
            }
        }
        Done:;
    }

    private static void UpdateBullets(GameState state, float dt)
    {
        // Player bullets: move toward horizon (increasing depth) + optional X drift
        foreach (var b in state.PlayerBullets)
        {
            b.Depth  += b.Speed     * dt;
            b.WorldX += b.VelocityX * dt;
            if (b.Depth > GameConstants.MaxDepth ||
                b.WorldX < -GameConstants.WorldHalfWidth * 1.5f ||
                b.WorldX >  GameConstants.WorldHalfWidth * 1.5f)
                b.IsDestroyed = true;
        }

        // Enemy bullets move toward near (decreasing depth)
        foreach (var b in state.EnemyBullets)
        {
            b.Depth -= b.Speed * dt;
            if (b.Depth < 0f) b.IsDestroyed = true;
        }
    }

    private static void UpdateEnemies(GameState state, float dt)
    {
        bool frozen = HasActiveEffect(state, PowerUpType.FreezeEnemies);
        bool slowed = HasActiveEffect(state, PowerUpType.SlowEnemies);
        float speedMul = frozen ? 0f : slowed ? 0.4f : 1f;

        foreach (var e in state.Enemies)
        {
            e.Depth -= e.Speed * speedMul * dt;  // zombies shuffle toward player

            // Past the player — overrun the crowd
            if (e.Depth < 0f)
            {
                e.IsDestroyed = true;

                // Shield absorbs the hit
                if (HasActiveEffect(state, PowerUpType.Shield))
                {
                    RemoveActiveEffect(state, PowerUpType.Shield);
                }
                else
                {
                    state.Crowd.Remove(e.CrowdDamage);
                    state.ScreenShake.AddTrauma(0.3f);
                }
            }
        }
    }

    /// <summary>Move gates based on their movement style.</summary>
    private static void UpdateGateMovement(GameState state, float dt)
    {
        float defaultSpeed = state.ActiveMap?.GateScrollSpeed ?? GameConstants.GateScrollSpeed;

        foreach (var g in state.Gates)
        {
            float speed = g.ScrollSpeed > 0 ? g.ScrollSpeed : defaultSpeed;

            switch (g.Movement)
            {
                case GateMovement.FastScroll:
                    g.Depth -= speed * dt;
                    break;

                case GateMovement.ScrollWithWorld:
                    g.Depth -= GameConstants.EnemySpeed * 0.6f * dt;
                    break;

                case GateMovement.Oscillate:
                    g.Depth -= speed * dt;
                    g.OscillateTimer += dt;
                    float amplitude = GameConstants.LaneWidth * 0.3f;
                    g.WorldX = g.LaneCenterX + amplitude * (float)System.Math.Sin(g.OscillateTimer * GameConstants.GateOscillateFrequency);
                    break;

                case GateMovement.Static:
                    break;
            }

            if (g.Depth < -GameConstants.GateDepth) g.IsDestroyed = true;
        }
    }

    /// <summary>Scroll power-ups toward the player and animate their bob timer.</summary>
    private static void UpdatePowerUps(GameState state, float dt)
    {
        float defaultSpeed = state.ActiveMap?.GateScrollSpeed ?? GameConstants.GateScrollSpeed;
        foreach (var p in state.PowerUps)
        {
            p.Depth     -= defaultSpeed * dt;
            p.BobTimer  += dt;
            // Update WorldHeight for the bobbing animation
            p.WorldHeight = 20f + 10f * System.MathF.Sin(p.BobTimer * 2.5f);
            if (p.Depth < -GameConstants.GateDepth) p.IsDestroyed = true;
        }
    }

    /// <summary>Scroll barriers toward the player.</summary>
    private static void UpdateBarriers(GameState state, float dt)
    {
        float defaultSpeed = state.ActiveMap?.GateScrollSpeed ?? GameConstants.GateScrollSpeed;
        foreach (var b in state.Barriers)
        {
            float speed = b.ScrollSpeed > 0 ? b.ScrollSpeed : defaultSpeed;
            b.Depth -= speed * dt;
            if (b.Depth < -GameConstants.GateDepth) b.IsDestroyed = true;
        }
    }

    /// <summary>Tick active effects and remove expired ones.</summary>
    private static void UpdateActiveEffects(GameState state, float dt)
    {
        foreach (var effect in state.ActiveEffects)
            effect.Elapsed += dt;
        state.ActiveEffects.RemoveAll(e => e.IsExpired);
    }

    private static void UpdateEffects(GameState state, float dt)
    {
        state.ScreenShake.Update(dt);

        foreach (var ft in state.FloatingTexts)
        {
            ft.Elapsed  += dt;
            ft.ScreenY  -= 50f * dt;
        }

        foreach (var p in state.Particles)
        {
            p.Elapsed  += dt;
            p.ScreenX  += p.VelocityX * dt;
            p.ScreenY  += p.VelocityY * dt;
            p.VelocityY += 120f * dt;
        }
    }

    private void UpdateSpawn(GameState state, float dt)
    {
        _spawn.Update(state, dt);
    }

    private static void CheckCollisions(GameState state)
    {
        var player = state.Player;

        // Player bullets vs enemies
        foreach (var bullet in state.PlayerBullets)
        {
            if (bullet.IsDestroyed) continue;
            foreach (var enemy in state.Enemies)
            {
                if (enemy.IsDestroyed || !bullet.Overlaps(enemy)) continue;

                enemy.Health -= 1f;

                if (!HasActiveEffect(state, PowerUpType.BulletPierce))
                    bullet.IsDestroyed = true;

                state.Score += 10;

                if (enemy.Health <= 0f)
                {
                    enemy.IsDestroyed = true;
                    state.Score += 50;
                    SpawnDeathEffects(state, enemy);
                }
                break;
            }
        }

        // Player bullets vs barriers
        foreach (var bullet in state.PlayerBullets)
        {
            if (bullet.IsDestroyed) continue;
            foreach (var barrier in state.Barriers)
            {
                if (barrier.IsDestroyed) continue;
                // Simple depth + X overlap check for barriers (rectangular)
                float halfW = barrier.Width * 0.5f;
                if (System.Math.Abs(bullet.WorldX - barrier.WorldX) > halfW) continue;
                if (System.Math.Abs(bullet.Depth  - barrier.Depth)  > barrier.Height * 0.5f) continue;

                bullet.IsDestroyed = true;
                barrier.Health--;
                if (barrier.Health <= 0)
                {
                    barrier.IsDestroyed = true;
                    state.Score += 100;
                    SpawnDeathEffects(state, barrier);
                }
                break;
            }
        }

        // Player bullets vs closed gates (shootable gates)
        foreach (var bullet in state.PlayerBullets)
        {
            if (bullet.IsDestroyed) continue;
            foreach (var gate in state.Gates)
            {
                if (gate.IsDestroyed || gate.OnShot == GateHitBehavior.Nothing) continue;
                if (!bullet.Overlaps(gate)) continue;

                bullet.IsDestroyed = true;
                HandleGateHit(gate);
                break;
            }
        }

        // Player bullets vs blocked power-ups
        foreach (var bullet in state.PlayerBullets)
        {
            if (bullet.IsDestroyed) continue;
            foreach (var pu in state.PowerUps)
            {
                if (pu.IsDestroyed || !pu.IsBlocked) continue;
                if (!bullet.Overlaps(pu)) continue;

                bullet.IsDestroyed = true;
                HandlePowerUpBlockHit(pu);
                break;
            }
        }

        // Gates vs player
        foreach (var gate in state.Gates)
        {
            if (gate.IsDestroyed) continue;
            if (!gate.Overlaps(player)) continue;

            if (gate.IsOpen)
            {
                var screenPos = GetApproximateScreenPos(state, gate);
                ApplyGate(state, gate, screenPos.x, screenPos.y);
            }
            gate.IsDestroyed = true;
        }

        // Power-ups vs player
        foreach (var pu in state.PowerUps)
        {
            if (pu.IsDestroyed) continue;
            if (!pu.IsRevealed) continue;
            if (!pu.Overlaps(player)) continue;

            ApplyPowerUp(state, pu);
            pu.IsDestroyed = true;
        }

        // Barriers vs player (barrier reaches the crowd → crush soldiers)
        foreach (var barrier in state.Barriers)
        {
            if (barrier.IsDestroyed) continue;
            if (barrier.Depth > player.Depth + player.Radius * 2f) continue;
            if (System.Math.Abs(barrier.WorldX - player.WorldX) > barrier.Width * 0.5f + GameConstants.CrowdHalfWidth) continue;

            // Barrier has reached the crowd
            if (HasActiveEffect(state, PowerUpType.Shield))
                RemoveActiveEffect(state, PowerUpType.Shield);
            else
            {
                state.Crowd.Remove(barrier.CrowdDamage);
                state.ScreenShake.AddTrauma(0.6f);
            }
            barrier.IsDestroyed = true;
        }
    }

    private static void HandleGateHit(Gate gate)
    {
        switch (gate.OnShot)
        {
            case GateHitBehavior.Open:
                gate.HitsRemaining--;
                if (gate.HitsRemaining <= 0) gate.IsOpen = true;
                break;

            case GateHitBehavior.Close:
                gate.IsOpen = false;
                break;

            case GateHitBehavior.Toggle:
                gate.IsOpen = !gate.IsOpen;
                break;

            case GateHitBehavior.Destroy:
                gate.HitsRemaining--;
                if (gate.HitsRemaining <= 0) gate.IsDestroyed = true;
                break;

            case GateHitBehavior.IncrementCounter:
                gate.HitCounter++;
                if (gate.HitCounter >= gate.HitsRemaining)
                    gate.IsOpen = true;
                break;
        }
    }

    private static void HandlePowerUpBlockHit(PowerUp pu)
    {
        switch (pu.OnShot)
        {
            case BlockHitBehavior.BreakConcrete:
                pu.BlockHitsRemaining--;
                if (pu.BlockHitsRemaining <= 0) pu.IsBlocked = false;
                break;

            case BlockHitBehavior.IncrementCounter:
                pu.HitCounter++;
                if (pu.HitCounter >= pu.CounterThreshold) pu.IsBlocked = false;
                break;
        }
    }

    private static void ApplyPowerUp(GameState state, PowerUp pu)
    {
        switch (pu.Type)
        {
            case PowerUpType.ExtraSoldiers:
                state.Crowd.Count += 5;
                break;
            case PowerUpType.GunUpgrade:
                state.Player.GunLevel++;
                break;
            default:
                if (pu.Duration > 0f)
                {
                    state.ActiveEffects.Add(new ActiveEffect
                    {
                        Type     = pu.Type,
                        Duration = pu.Duration
                    });
                }
                break;
        }

        var screenPos = GetApproximateScreenPos(state, pu);
        state.FloatingTexts.Add(new FloatingText
        {
            Text    = pu.Label,
            ScreenX = screenPos.x,
            ScreenY = screenPos.y - 30f
        });
    }

    private static void SpawnDeathEffects(GameState state, GameObjectBase obj)
    {
        var camera = new Math.Camera(state.ViewportWidth, state.ViewportHeight);
        var (sx, sy, _) = camera.Project(obj.WorldX, obj.Depth);

        state.Particles.AddRange(Particle.Burst(sx, sy));
        state.FloatingTexts.Add(new FloatingText { Text = "+50", ScreenX = sx, ScreenY = sy - 20f });
    }

    private static void ApplyGate(GameState state, Gate gate, float screenX, float screenY)
    {
        string label = gate.Label;
        gate.Apply(state);
        state.FloatingTexts.Add(new FloatingText { Text = label, ScreenX = screenX, ScreenY = screenY - 30f });
    }

    private static (float x, float y) GetApproximateScreenPos(GameState state, GameObjectBase obj)
    {
        var camera = new Math.Camera(state.ViewportWidth, state.ViewportHeight);
        var (sx, sy, _) = camera.Project(obj.WorldX, obj.Depth);
        return (sx, sy);
    }

    private static void PruneDestroyed(GameState state)
    {
        state.Enemies.RemoveAll(e => e.IsDestroyed);
        state.PlayerBullets.RemoveAll(b => b.IsDestroyed);
        state.EnemyBullets.RemoveAll(b => b.IsDestroyed);
        state.Gates.RemoveAll(g => g.IsDestroyed);
        state.Obstacles.RemoveAll(o => o.IsDestroyed);
        state.PowerUps.RemoveAll(p => p.IsDestroyed);
        state.Barriers.RemoveAll(b => b.IsDestroyed);
        state.FloatingTexts.RemoveAll(ft => ft.IsExpired);
        state.Particles.RemoveAll(p => p.IsExpired);
    }

    private static void CheckEndConditions(GameState state)
    {
        if (state.Crowd.IsEmpty)
            state.Phase = GamePhase.GameOver;
    }

    // ── Active effect helpers ────────────────────────────────────────────────

    private static bool HasActiveEffect(GameState state, PowerUpType type)
    {
        foreach (var e in state.ActiveEffects)
            if (e.Type == type && !e.IsExpired) return true;
        return false;
    }

    private static void RemoveActiveEffect(GameState state, PowerUpType type)
    {
        for (int i = state.ActiveEffects.Count - 1; i >= 0; i--)
        {
            if (state.ActiveEffects[i].Type == type)
            {
                state.ActiveEffects.RemoveAt(i);
                return;
            }
        }
    }
}
