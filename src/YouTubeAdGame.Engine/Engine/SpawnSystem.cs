using YouTubeAdGame.Engine.Core;
using YouTubeAdGame.Engine.Maps;
using YouTubeAdGame.Engine.Objects;

namespace YouTubeAdGame.Engine.Engine;

/// <summary>
/// Handles timed spawning of enemies, bosses, gates, obstacles, and power-ups.
/// Reads gate/power-up palettes from <see cref="GameState.ActiveMap"/> when available,
/// falling back to hard-coded defaults when no map is loaded.
/// </summary>
internal sealed class SpawnSystem
{
    private static readonly Random Rng = Random.Shared;

    public void Update(GameState state, float dt)
    {
        var map = state.ActiveMap;

        // Enemies
        state.EnemySpawnTimer -= dt;
        if (state.EnemySpawnTimer <= 0f)
        {
            SpawnZombieBatch(state);
            // Use the runtime-adjustable interval; wave pressure gradually increases spawn rate
            float interval = System.Math.Max(0.05f,
                state.SpawnInterval - state.Wave * 0.004f);
            state.EnemySpawnTimer = interval;
        }

        // Gates
        float gateInterval = map?.GateSpawnInterval ?? GameConstants.GateSpawnInterval;
        state.GateSpawnTimer -= dt;
        if (state.GateSpawnTimer <= 0f)
        {
            SpawnGateRow(state);
            state.GateSpawnTimer = gateInterval;
        }

        // Power-ups
        float powerUpInterval = map?.PowerUpSpawnInterval ?? 0f;
        if (powerUpInterval > 0f && map?.PowerUpPalette.Count > 0)
        {
            state.PowerUpSpawnTimer -= dt;
            if (state.PowerUpSpawnTimer <= 0f)
            {
                SpawnPowerUp(state, map);
                state.PowerUpSpawnTimer = powerUpInterval;
            }
        }

        // Advance wave every 1800 distance units
        state.Wave = 1 + (int)(state.Distance / 1800f);
    }

    /// <summary>
    /// No initial horde — enemies stream in one at a time from the horizon.
    /// </summary>
    public void SpawnInitialHorde(GameState state)
    {
        // Intentionally empty: the horde streams in via SpawnEnemyWave.
    }

    private static void SpawnZombieBatch(GameState state)
    {
        // Spawn a whole batch of zombies at the horizon to create a sea effect
        int capacity = state.MaxEnemiesOnScreen - state.Enemies.Count;
        if (capacity <= 0) return;

        int toSpawn = System.Math.Min(GameConstants.ZombiesPerSpawn, capacity);
        float spread = GameConstants.WorldHalfWidth - GameConstants.EnemyRadius;

        for (int i = 0; i < toSpawn; i++)
        {
            float x = (float)(Rng.NextDouble() * 2.0 - 1.0) * spread;
            float depthJitter = (float)(Rng.NextDouble() * 30.0);  // stagger depth slightly
            state.Enemies.Add(new Enemy
            {
                WorldX = x,
                Depth  = GameConstants.SpawnDepth - depthJitter,
                Speed  = GameConstants.EnemySpeed + (float)(Rng.NextDouble() * 10.0 - 5.0)
            });
        }
    }

    private static void SpawnGateRow(GameState state)
    {
        var map = state.ActiveMap;
        int laneCount = map?.LaneCount ?? GameConstants.LaneCount;
        float laneWidth = map?.LaneWidth ?? GameConstants.LaneWidth;
        float depth = GameConstants.SpawnDepth - 30f;

        for (int lane = 0; lane < laneCount; lane++)
        {
            float laneCenter = -GameConstants.WorldHalfWidth
                + laneWidth * 0.5f
                + lane * laneWidth;

            bool isLeftLane = lane == 0;

            if (map is not null && map.GatePalette.Count > 0)
            {
                var def = PickGateDefinition(map, state.Wave, isLeftLane);
                if (def is null) continue;

                int operand = def.OperandRange.Min == def.OperandRange.Max
                    ? def.OperandRange.Min
                    : Rng.Next(def.OperandRange.Min, def.OperandRange.Max + 1);

                float scrollSpeed = def.ScrollSpeed ?? map.GateScrollSpeed;

                state.Gates.Add(new Gate
                {
                    WorldX       = laneCenter,
                    Depth        = depth,
                    Operation    = def.Operation,
                    Operand      = operand,
                    IsLeftLane   = isLeftLane,
                    Movement     = def.MovementStyle,
                    ScrollSpeed  = scrollSpeed,
                    IsOpen       = def.IsOpen,
                    OnShot       = def.OnShot,
                    HitsRemaining = def.HitsToOpen,
                    LaneCenterX  = laneCenter
                });
            }
            else
            {
                // Fallback: original hard-coded behaviour
                var (op, operand) = ChooseOperationFallback(state, leftLane: isLeftLane);
                state.Gates.Add(new Gate
                {
                    WorldX     = laneCenter,
                    Depth      = depth,
                    Operation  = op,
                    Operand    = operand,
                    IsLeftLane = isLeftLane,
                    LaneCenterX = laneCenter
                });
            }
        }
    }

    /// <summary>
    /// Pick a gate definition from the map palette using weighted random selection,
    /// filtered by the current wave and lane constraints.
    /// </summary>
    private static GateDefinition? PickGateDefinition(MapDefinition map, int wave, bool isLeftLane)
    {
        // Cache available gates to avoid iterating the palette twice
        List<GateDefinition>? available = null;
        float totalWeight = 0f;

        foreach (var g in map.GatePalette)
        {
            if (!IsAvailable(g, wave, isLeftLane)) continue;
            available ??= [];
            available.Add(g);
            totalWeight += g.SpawnWeight;
        }

        if (available is null || totalWeight <= 0f) return null;

        float roll = (float)(Rng.NextDouble() * totalWeight);
        float cumulative = 0f;
        foreach (var g in available)
        {
            cumulative += g.SpawnWeight;
            if (roll < cumulative) return g;
        }

        // Return the last available gate (always valid since we filtered above)
        return available[^1];
    }

    private static bool IsAvailable(GateDefinition def, int wave, bool isLeftLane)
    {
        if (def.MinWave.HasValue && wave < def.MinWave.Value) return false;
        if (def.MaxWave.HasValue && wave > def.MaxWave.Value) return false;
        if (def.LeftLaneOnly && !isLeftLane) return false;
        return true;
    }

    /// <summary>
    /// Spawn a single power-up in a random lane at the horizon.
    /// </summary>
    private static void SpawnPowerUp(GameState state, MapDefinition map)
    {
        var def = PickPowerUpDefinition(map, state.Wave);
        if (def is null) return;

        int laneCount = map.LaneCount;
        float laneWidth = map.LaneWidth;
        int lane = Rng.Next(laneCount);
        float laneCenter = -GameConstants.WorldHalfWidth
            + laneWidth * 0.5f
            + lane * laneWidth;

        state.PowerUps.Add(new PowerUp
        {
            WorldX            = laneCenter,
            Depth             = GameConstants.SpawnDepth - 30f,
            Type              = def.Type,
            Duration          = def.Duration,
            IsBlocked         = def.IsBlocked,
            BlockHitsRemaining = def.BlockHealth,
            OnShot            = def.OnShot,
            CounterThreshold  = def.CounterThreshold
        });
    }

    private static PowerUpDefinition? PickPowerUpDefinition(MapDefinition map, int wave)
    {
        List<PowerUpDefinition>? available = null;
        float totalWeight = 0f;

        foreach (var p in map.PowerUpPalette)
        {
            if (!IsAvailable(p, wave)) continue;
            available ??= [];
            available.Add(p);
            totalWeight += p.SpawnWeight;
        }

        if (available is null || totalWeight <= 0f) return null;

        float roll = (float)(Rng.NextDouble() * totalWeight);
        float cumulative = 0f;
        foreach (var p in available)
        {
            cumulative += p.SpawnWeight;
            if (roll < cumulative) return p;
        }

        return available[^1];
    }

    private static bool IsAvailable(PowerUpDefinition def, int wave)
    {
        if (def.MinWave.HasValue && wave < def.MinWave.Value) return false;
        if (def.MaxWave.HasValue && wave > def.MaxWave.Value) return false;
        return true;
    }

    /// <summary>
    /// Original hard-coded gate choice logic — used when no ActiveMap is loaded.
    /// </summary>
    private static (GateOperation op, int operand) ChooseOperationFallback(GameState state, bool leftLane)
    {
        int crowd = state.Crowd.Count;
        int wave  = state.Wave;

        // Gun upgrade chance scales up with wave (max 25%)
        float gunChance = System.Math.Min(0.10f + wave * 0.02f, 0.25f);
        if (leftLane && Rng.NextDouble() < gunChance)
            return (GateOperation.UpgradeGun, 0);

        // Gate operands scale with wave: small values early, larger as game progresses
        if (wave <= 2)
        {
            GateOperation[] ops = crowd <= 5
                ? [GateOperation.Add, GateOperation.Add, GateOperation.Multiply]
                : [GateOperation.Add, GateOperation.Multiply, GateOperation.Subtract];

            var op = ops[Rng.Next(ops.Length)];
            int operand = op switch
            {
                GateOperation.Add      => Rng.Next(1, 3),
                GateOperation.Subtract => 1,
                GateOperation.Multiply => 2,
                _                      => 0
            };
            return (op, operand);
        }
        else if (wave <= 5)
        {
            GateOperation[] ops = crowd >= 10
                ? [GateOperation.Add, GateOperation.Multiply, GateOperation.Subtract]
                : [GateOperation.Add, GateOperation.Add, GateOperation.Multiply];

            var op = ops[Rng.Next(ops.Length)];
            int operand = op switch
            {
                GateOperation.Add      => Rng.Next(2, 6),
                GateOperation.Subtract => Rng.Next(1, 3),
                GateOperation.Multiply => Rng.Next(2, 4),
                _                      => 0
            };
            return (op, operand);
        }
        else
        {
            GateOperation[] ops = crowd >= 20
                ? [GateOperation.Add, GateOperation.Multiply, GateOperation.Subtract]
                : [GateOperation.Add, GateOperation.Add, GateOperation.Multiply];

            var op = ops[Rng.Next(ops.Length)];
            int operand = op switch
            {
                GateOperation.Add      => Rng.Next(5, 25),
                GateOperation.Subtract => Rng.Next(2, 10),
                GateOperation.Multiply => Rng.Next(2, 4),
                _                      => 0
            };
            return (op, operand);
        }
    }
}
