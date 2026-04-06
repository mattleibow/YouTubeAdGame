using YouTubeAdGame.Engine.Core;
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

    // ── Public API ──────────────────────────────────────────────────────────

    /// <summary>Start a new game session.</summary>
    public void StartGame(GameState state)
    {
        state.Phase = GamePhase.Playing;
        state.Score = 0;
        state.Wave  = 1;
        state.Distance = 0;
        state.ScrollOffset = 0;
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
        state.FloatingTexts.Clear();
        state.Particles.Clear();
        state.EnemySpawnTimer = 0;
        state.GateSpawnTimer  = GameConstants.GateSpawnInterval * 0.5f;
        state.PlayerFireTimer = 0;

        _spawn.SpawnInitialHorde(state);
    }

    /// <summary>
    /// Advance the simulation by <paramref name="dt"/> seconds.
    /// Call on every fixed-timestep tick (60 Hz).
    /// </summary>
    public void Update(GameState state, float dt)
    {
        if (state.Phase != GamePhase.Playing) return;

        var inputState = input.Poll();

        UpdatePlayer(state, inputState, dt);
        UpdateBullets(state, dt);
        UpdateEnemies(state, dt);
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

        player.VelocityX += inputState.HorizontalAxis * accel * dt;
        player.VelocityX *= drag;

        // Move and clamp to road bounds — keep the full crowd formation on-road
        player.WorldX += player.VelocityX * dt;
        player.WorldX = System.Math.Clamp(player.WorldX,
            -GameConstants.WorldHalfWidth + GameConstants.CrowdHalfWidth,
             GameConstants.WorldHalfWidth - GameConstants.CrowdHalfWidth);

        // Auto-fire
        player.HitFlashTimer = System.Math.Max(0f, player.HitFlashTimer - dt);
        state.PlayerFireTimer -= dt;
        if (state.PlayerFireTimer <= 0f)
        {
            FirePlayerBullets(state);
            state.PlayerFireTimer = GameConstants.PlayerFireRate;
        }

        // Scroll world forward
        state.ScrollOffset += GameConstants.EnemySpeed * 0.6f * dt;
        state.Distance += GameConstants.EnemySpeed * 0.6f * dt;
    }

    private static void FirePlayerBullets(GameState state)
    {
        // More crowd members = more bullets, spread across the formation width.
        // Gun level acts as a density multiplier (gate reward).
        int baseBullets = System.Math.Max(1, state.Crowd.Count / 5);
        float densityMul = 1f + state.Player.GunLevel * 0.5f;
        int count = System.Math.Min((int)(baseBullets * densityMul), 50);

        float spreadHalf = GameConstants.CrowdHalfWidth;
        float depth = state.Player.Depth;

        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0.5f : (float)i / (count - 1);
            float worldX = state.Player.WorldX + (t * 2f - 1f) * spreadHalf;
            state.PlayerBullets.Add(new Bullet(BulletOwner.Player, worldX, depth, GameConstants.BulletSpeed));
        }
    }

    private static void UpdateBullets(GameState state, float dt)
    {
        // Player bullets move toward far (increasing depth)
        foreach (var b in state.PlayerBullets)
        {
            b.Depth += b.Speed * dt;
            if (b.Depth > GameConstants.MaxDepth) b.IsDestroyed = true;
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
        foreach (var e in state.Enemies)
        {
            e.Depth -= e.Speed * dt;  // zombies shuffle toward player

            // Past the player — overrun the crowd
            if (e.Depth < 0f)
            {
                e.IsDestroyed = true;
                state.Crowd.Remove(e.CrowdDamage);
                state.ScreenShake.AddTrauma(0.3f);
            }
        }

        // Gates scroll with the world
        foreach (var g in state.Gates)
        {
            g.Depth -= GameConstants.EnemySpeed * 0.6f * dt;
            if (g.Depth < -GameConstants.GateDepth) g.IsDestroyed = true;
        }

        // Obstacles scroll with the world
        foreach (var o in state.Obstacles)
        {
            o.Depth -= GameConstants.EnemySpeed * 0.6f * dt;
            if (o.Depth < 0f) o.IsDestroyed = true;
        }
    }

    private static void UpdateEffects(GameState state, float dt)
    {
        state.ScreenShake.Update(dt);

        foreach (var ft in state.FloatingTexts)
        {
            ft.Elapsed  += dt;
            ft.ScreenY  -= 50f * dt;  // float upward
        }

        foreach (var p in state.Particles)
        {
            p.Elapsed  += dt;
            p.ScreenX  += p.VelocityX * dt;
            p.ScreenY  += p.VelocityY * dt;
            p.VelocityY += 120f * dt;  // gravity
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

        // Enemy bullets vs player — zombies don't shoot, list is always empty
        // (collision block retained for completeness; no-ops at runtime)

        // Gates vs player
        foreach (var gate in state.Gates)
        {
            if (gate.IsDestroyed) continue;
            if (!gate.Overlaps(player)) continue;

            var screenPos = GetApproximateScreenPos(state, gate);
            ApplyGate(state, gate, screenPos.x, screenPos.y);
            gate.IsDestroyed = true;
        }
    }

    private static void HitPlayer(GameState state)
    {
        state.Player.Health--;
        state.Player.HitFlashTimer = 0.3f;
        state.ScreenShake.AddTrauma(0.5f);
        if (state.Player.Health <= 0) state.Phase = GamePhase.GameOver;
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
        state.FloatingTexts.RemoveAll(ft => ft.IsExpired);
        state.Particles.RemoveAll(p => p.IsExpired);
    }

    private static void CheckEndConditions(GameState state)
    {
        // Game over when the crowd is wiped out — zombies have overrun the army
        if (state.Crowd.IsEmpty)
            state.Phase = GamePhase.GameOver;
    }
}
