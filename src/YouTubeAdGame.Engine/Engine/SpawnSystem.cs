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
            // Use the runtime-adjustable interval; a small wave bonus keeps pressure rising
            float interval = System.Math.Max(0.05f,
                state.SpawnInterval - state.Wave * 0.002f);
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
                Speed  = GameConstants.EnemySpeed + (float)(Rng.NextDouble() * 20.0 - 10.0) + state.Wave * 2f
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
        // Mix of operations based on crowd size
        int crowd = state.Crowd.Count;

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

        // Occasionally offer gun upgrade on the left lane
        if (leftLane && Rng.NextDouble() < 0.15)
            return (GateOperation.UpgradeGun, 0);

        return (op, operand);
    }
}
