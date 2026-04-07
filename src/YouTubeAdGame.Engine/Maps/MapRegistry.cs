using YouTubeAdGame.Engine.Core;
using YouTubeAdGame.Engine.Objects;

namespace YouTubeAdGame.Engine.Maps;

/// <summary>
/// Registry that maps each <see cref="GameMode"/> to its <see cref="MapDefinition"/>.
/// New modes are added here — no engine/spawn/renderer changes required.
/// </summary>
public static class MapRegistry
{
    /// <summary>
    /// Returns the <see cref="MapDefinition"/> for the given <paramref name="mode"/>.
    /// </summary>
    public static MapDefinition Get(GameMode mode) => mode switch
    {
        GameMode.HordeRunner => HordeRunner,
        _ => HordeRunner
    };

    /// <summary>
    /// Classic horde runner: 3-lane road, fast-scrolling gates, vast zombie hordes.
    /// Gate palette reproduces the existing wave-scaled operand ranges.
    /// </summary>
    public static MapDefinition HordeRunner { get; } = new()
    {
        Name        = "Horde Runner",
        Description = "3 lanes / fast gates / vast hordes",

        LaneCount   = GameConstants.LaneCount,
        LaneWidth   = GameConstants.LaneWidth,

        PlayerSpeed = GameConstants.PlayerSpeed,
        BulletSpeed = GameConstants.BulletSpeed,
        FireRate    = GameConstants.PlayerFireRate,

        EnemySpeed     = GameConstants.EnemySpeed,
        MaxEnemies     = GameConstants.MaxEnemiesOnScreen,
        SpawnInterval  = GameConstants.SpawnInterval,
        ZombiesPerSpawn = GameConstants.ZombiesPerSpawn,

        GateScrollSpeed   = GameConstants.GateScrollSpeed,
        GateSpawnInterval = GameConstants.GateSpawnInterval,

        PowerUpSpawnInterval = 12f,   // power-ups every 12 seconds

        GatePalette =
        [
            // ── Early-game gates (waves 1–2) ──────────────────────────────
            new GateDefinition
            {
                Operation    = GateOperation.Add,
                OperandRange = (1, 2),
                SpawnWeight  = 2f,
                MaxWave      = 2
            },
            new GateDefinition
            {
                Operation    = GateOperation.Multiply,
                OperandRange = (2, 2),
                SpawnWeight  = 1f,
                MaxWave      = 2
            },
            new GateDefinition
            {
                Operation    = GateOperation.Subtract,
                OperandRange = (1, 1),
                SpawnWeight  = 0.5f,
                MaxWave      = 2
            },

            // ── Mid-game gates (waves 3–5) ────────────────────────────────
            new GateDefinition
            {
                Operation    = GateOperation.Add,
                OperandRange = (2, 5),
                SpawnWeight  = 2f,
                MinWave      = 3,
                MaxWave      = 5
            },
            new GateDefinition
            {
                Operation    = GateOperation.Multiply,
                OperandRange = (2, 3),
                SpawnWeight  = 1f,
                MinWave      = 3,
                MaxWave      = 5
            },
            new GateDefinition
            {
                Operation    = GateOperation.Subtract,
                OperandRange = (1, 2),
                SpawnWeight  = 0.8f,
                MinWave      = 3,
                MaxWave      = 5
            },

            // ── Late-game gates (wave 6+) ─────────────────────────────────
            new GateDefinition
            {
                Operation    = GateOperation.Add,
                OperandRange = (5, 24),
                SpawnWeight  = 2f,
                MinWave      = 6
            },
            new GateDefinition
            {
                Operation    = GateOperation.Multiply,
                OperandRange = (2, 3),
                SpawnWeight  = 1f,
                MinWave      = 6
            },
            new GateDefinition
            {
                Operation    = GateOperation.Subtract,
                OperandRange = (2, 9),
                SpawnWeight  = 0.8f,
                MinWave      = 6
            },

            // ── Gun upgrade (all waves, left lane only) ───────────────────
            new GateDefinition
            {
                Operation    = GateOperation.UpgradeGun,
                OperandRange = (0, 0),
                SpawnWeight  = 0.5f,
                LeftLaneOnly = true
            }
        ],

        PowerUpPalette =
        [
            new PowerUpDefinition
            {
                Type        = PowerUpType.RapidFire,
                Duration    = 5f,
                SpawnWeight = 1f,
                MinWave     = 2
            },
            new PowerUpDefinition
            {
                Type        = PowerUpType.Shield,
                Duration    = 6f,
                SpawnWeight = 1f,
                MinWave     = 3
            },
            new PowerUpDefinition
            {
                Type        = PowerUpType.SpeedBoost,
                Duration    = 4f,
                SpawnWeight = 0.8f
            },
            new PowerUpDefinition
            {
                Type        = PowerUpType.SlowEnemies,
                Duration    = 4f,
                SpawnWeight = 0.6f,
                MinWave     = 4
            },
            new PowerUpDefinition
            {
                Type         = PowerUpType.ExtraSoldiers,
                Duration     = 0f,   // instant
                IsBlocked    = true,
                BlockHealth  = 5,
                OnShot       = BlockHitBehavior.BreakConcrete,
                SpawnWeight  = 0.4f,
                MinWave      = 3
            }
        ],

        Waves = []  // use automatic wave scaling from the map defaults
    };
}
