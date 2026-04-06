using YouTubeAdGame.Engine.Core;
using YouTubeAdGame.Engine.Objects;

namespace YouTubeAdGame.Engine.Engine;

/// <summary>
/// Handles timed spawning of enemies, bosses, gates, and obstacles.
/// </summary>
internal sealed class SpawnSystem
{
    private static readonly Random Rng = Random.Shared;

    public void Update(GameState state, float dt)
    {
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
        state.GateSpawnTimer -= dt;
        if (state.GateSpawnTimer <= 0f)
        {
            SpawnGatePair(state);
            state.GateSpawnTimer = GameConstants.GateSpawnInterval;
        }

        // Advance wave every 15 seconds
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

    private static void SpawnGatePair(GameState state)
    {
        // Always spawn two gates — one on each side of the road
        float depth = GameConstants.SpawnDepth - 30f;
        float laneOffset = GameConstants.WorldHalfWidth * 0.45f;

        var leftOp  = ChooseOperation(state, leftLane: true);
        var rightOp = ChooseOperation(state, leftLane: false);

        state.Gates.Add(new Gate
        {
            WorldX      = -laneOffset,
            Depth       = depth,
            Operation   = leftOp.op,
            Operand     = leftOp.operand,
            IsLeftLane  = true,
            Radius      = GameConstants.GateWidth * 0.5f
        });

        state.Gates.Add(new Gate
        {
            WorldX      = laneOffset,
            Depth       = depth,
            Operation   = rightOp.op,
            Operand     = rightOp.operand,
            IsLeftLane  = false,
            Radius      = GameConstants.GateWidth * 0.5f
        });
    }

    private static (GateOperation op, int operand) ChooseOperation(GameState state, bool leftLane)
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
            // Early game: only small additions and doubling
            GateOperation[] ops = crowd <= 5
                ? [GateOperation.Add, GateOperation.Add, GateOperation.Multiply]
                : [GateOperation.Add, GateOperation.Multiply, GateOperation.Subtract];

            var op = ops[Rng.Next(ops.Length)];
            int operand = op switch
            {
                GateOperation.Add      => Rng.Next(1, 3),    // +1 or +2
                GateOperation.Subtract => 1,                  // -1
                GateOperation.Multiply => 2,                  // ×2
                _                      => 0
            };
            return (op, operand);
        }
        else if (wave <= 5)
        {
            // Mid game: slightly larger gains
            GateOperation[] ops = crowd >= 10
                ? [GateOperation.Add, GateOperation.Multiply, GateOperation.Subtract]
                : [GateOperation.Add, GateOperation.Add, GateOperation.Multiply];

            var op = ops[Rng.Next(ops.Length)];
            int operand = op switch
            {
                GateOperation.Add      => Rng.Next(2, 6),    // +2 to +5
                GateOperation.Subtract => Rng.Next(1, 3),    // -1 or -2
                GateOperation.Multiply => Rng.Next(2, 4),    // ×2 or ×3
                _                      => 0
            };
            return (op, operand);
        }
        else
        {
            // Late game: current large-value behaviour
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
