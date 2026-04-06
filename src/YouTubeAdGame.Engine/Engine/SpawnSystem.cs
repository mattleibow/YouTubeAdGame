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
            SpawnEnemyWave(state);
            // Gradually reduce interval as waves progress
            float interval = System.Math.Max(0.8f,
                GameConstants.SpawnInterval - state.Wave * 0.1f);
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

    private static void SpawnEnemyWave(GameState state)
    {
        int count = 1 + (int)(state.Wave * 0.5f);
        count = System.Math.Min(count, 5);

        for (int i = 0; i < count; i++)
        {
            float x = (float)(Rng.NextDouble() * 2.0 - 1.0)
                      * (GameConstants.WorldHalfWidth - GameConstants.EnemyRadius);
            state.Enemies.Add(new Enemy
            {
                WorldX = x,
                Depth  = GameConstants.SpawnDepth - i * 60f,
                Speed  = GameConstants.EnemySpeed + Rng.Next(0, state.Wave * 10)
            });
        }

        // Spawn a boss every 5 waves (not yet fully implemented — placeholder)
        // if (state.Wave % 5 == 0) SpawnBoss(state);
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
