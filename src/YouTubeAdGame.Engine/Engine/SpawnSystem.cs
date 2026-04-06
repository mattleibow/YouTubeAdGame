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
            // Gradually reduce interval as waves progress, minimum 0.5 s
            float interval = System.Math.Max(0.5f,
                GameConstants.SpawnInterval - state.Wave * 0.05f);
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
    /// Pre-populate a horde at the far distance so the player sees the crowd
    /// approaching from the horizon and has time to start firing.
    /// </summary>
    public void SpawnInitialHorde(GameState state)
    {
        const int count = 50;
        for (int i = 0; i < count; i++)
        {
            // Spawn in the far band of the view (780–980 depth) so the horde
            // is visible at a distance and walks toward the player
            float depth = GameConstants.SpawnDepth - (float)Rng.NextDouble() * 200f;
            float x     = (float)(Rng.NextDouble() * 2.0 - 1.0)
                          * (GameConstants.WorldHalfWidth - GameConstants.EnemyRadius);
            state.Enemies.Add(new Enemy
            {
                WorldX = x,
                Depth  = depth,
                Speed  = GameConstants.EnemySpeed + (float)(Rng.NextDouble() * 20.0 - 10.0)
            });
        }
    }

    private static void SpawnEnemyWave(GameState state)
    {
        // Scale horde size with wave — starts large, keeps growing
        int desired = System.Math.Min(10 + state.Wave * 3, 30);

        // Respect the on-screen cap so we don't flood the engine
        int available = GameConstants.MaxEnemiesOnScreen - state.Enemies.Count;
        int count = System.Math.Min(desired, available);
        if (count <= 0) return;

        for (int i = 0; i < count; i++)
        {
            float x = (float)(Rng.NextDouble() * 2.0 - 1.0)
                      * (GameConstants.WorldHalfWidth - GameConstants.EnemyRadius);
            // Stagger enemies at different depths so they arrive in waves, not a wall
            float depthJitter = (float)(Rng.NextDouble() * 150.0);
            state.Enemies.Add(new Enemy
            {
                WorldX = x,
                Depth  = GameConstants.SpawnDepth - depthJitter,
                // Small random ±10 speed variation around the base, always valid
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
