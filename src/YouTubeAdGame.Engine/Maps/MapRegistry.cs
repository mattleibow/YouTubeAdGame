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
    /// For <see cref="GameMode.Custom"/>, returns <see cref="HordeRunner"/> as the
    /// fallback; callers that have a user-defined map should pass it directly via
    /// <see cref="Core.GameState.ActiveMap"/> before calling <c>StartGame</c>.
    /// </summary>
    public static MapDefinition Get(GameMode mode) => mode switch
    {
        GameMode.HordeRunner => HordeRunner,
        GameMode.Blitz       => Blitz,
        GameMode.Survival    => Survival,
        GameMode.Custom      => HordeRunner,   // fallback only — caller should override
        _                    => HordeRunner
    };

    /// <summary>
    /// Returns all built-in (non-custom) modes in display order.
    /// </summary>
    public static IReadOnlyList<(GameMode Mode, MapDefinition Map)> GetAll() =>
    [
        (GameMode.HordeRunner, HordeRunner),
        (GameMode.Blitz,       Blitz),
        (GameMode.Survival,    Survival),
    ];

    // ────────────────────────────────────────────────────────────────────────
    // Preset: Horde Runner (default)
    // ────────────────────────────────────────────────────────────────────────

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

    // ────────────────────────────────────────────────────────────────────────
    // Preset: Blitz
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Blitz mode: lightning-fast zombies, rapid spawn, gates rush in at high speed.
    /// No subtract gates — pure growth. Power-ups appear frequently to compensate.
    /// </summary>
    public static MapDefinition Blitz { get; } = new()
    {
        Name        = "Blitz",
        Description = "Lightning fast zombies / rapid gates / no mercy",

        LaneCount   = GameConstants.LaneCount,
        LaneWidth   = GameConstants.LaneWidth,

        PlayerSpeed = GameConstants.PlayerSpeed * 1.25f,
        BulletSpeed = GameConstants.BulletSpeed * 1.2f,
        FireRate    = GameConstants.PlayerFireRate * 0.75f,  // 25% faster shooting

        EnemySpeed      = 80f,                     // 60% faster than default
        MaxEnemies      = 300,
        SpawnInterval   = 0.06f,                   // 60 ms — extremely rapid
        ZombiesPerSpawn = 12,

        GateScrollSpeed   = 200f,                  // gates rush in very fast
        GateSpawnInterval = 3f,                    // new gate row every 3 s

        PowerUpSpawnInterval = 8f,                 // power-ups every 8 s

        GatePalette =
        [
            // Early (waves 1–2): +2–4
            new GateDefinition { Operation = GateOperation.Add,      OperandRange = (2, 4),  SpawnWeight = 2f,  MaxWave = 2 },
            new GateDefinition { Operation = GateOperation.Multiply, OperandRange = (2, 2),  SpawnWeight = 1f,  MaxWave = 2 },

            // Mid (waves 3–5): +4–10, ×2–3
            new GateDefinition { Operation = GateOperation.Add,      OperandRange = (4, 10), SpawnWeight = 2f,  MinWave = 3, MaxWave = 5 },
            new GateDefinition { Operation = GateOperation.Multiply, OperandRange = (2, 3),  SpawnWeight = 1.5f,MinWave = 3, MaxWave = 5 },

            // Late (wave 6+): +10–30, ×3–5
            new GateDefinition { Operation = GateOperation.Add,      OperandRange = (10, 30),SpawnWeight = 2f,  MinWave = 6 },
            new GateDefinition { Operation = GateOperation.Multiply, OperandRange = (3, 5),  SpawnWeight = 1.5f,MinWave = 6 },

            // Gun upgrade — appears from wave 1 in blitz
            new GateDefinition { Operation = GateOperation.UpgradeGun, OperandRange = (0, 0), SpawnWeight = 0.8f, LeftLaneOnly = true }
        ],

        PowerUpPalette =
        [
            new PowerUpDefinition { Type = PowerUpType.RapidFire,     Duration = 6f,  SpawnWeight = 1.2f },
            new PowerUpDefinition { Type = PowerUpType.Shield,        Duration = 8f,  SpawnWeight = 1f   },
            new PowerUpDefinition { Type = PowerUpType.BulletPierce,  Duration = 5f,  SpawnWeight = 0.8f, MinWave = 2 },
            new PowerUpDefinition { Type = PowerUpType.FreezeEnemies, Duration = 3f,  SpawnWeight = 0.6f, MinWave = 3 },
            new PowerUpDefinition { Type = PowerUpType.ExtraSoldiers, Duration = 0f,  SpawnWeight = 0.5f,
                                    IsBlocked = true, BlockHealth = 3, OnShot = BlockHitBehavior.BreakConcrete }
        ],

        Waves = []
    };

    // ────────────────────────────────────────────────────────────────────────
    // Preset: Survival
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Survival mode: starts slow and escalates relentlessly each wave.
    /// Gates are rare and precious — every gate choice has lasting impact.
    /// </summary>
    public static MapDefinition Survival { get; } = new()
    {
        Name        = "Survival",
        Description = "Starts slow / gates are rare / never stops ramping",

        LaneCount   = GameConstants.LaneCount,
        LaneWidth   = GameConstants.LaneWidth,

        PlayerSpeed = GameConstants.PlayerSpeed,
        BulletSpeed = GameConstants.BulletSpeed,
        FireRate    = GameConstants.PlayerFireRate,

        EnemySpeed      = 25f,             // starts very slow
        MaxEnemies      = 800,             // huge cap — they pile up over time
        SpawnInterval   = 0.25f,           // 250 ms — relaxed early
        ZombiesPerSpawn = 4,

        GateScrollSpeed   = 120f,          // slower gates — more time to decide
        GateSpawnInterval = 8f,            // rare gates every 8 s

        PowerUpSpawnInterval = 20f,        // power-ups are uncommon

        GatePalette =
        [
            // All waves: solid additions and multipliers
            new GateDefinition { Operation = GateOperation.Add,      OperandRange = (3, 8),  SpawnWeight = 2.5f },
            new GateDefinition { Operation = GateOperation.Multiply, OperandRange = (2, 3),  SpawnWeight = 1.5f },
            new GateDefinition { Operation = GateOperation.Subtract, OperandRange = (1, 3),  SpawnWeight = 0.4f },
            new GateDefinition { Operation = GateOperation.UpgradeGun, OperandRange = (0, 0), SpawnWeight = 0.6f, LeftLaneOnly = true },

            // Late (wave 5+): big multipliers
            new GateDefinition { Operation = GateOperation.Multiply, OperandRange = (4, 6),  SpawnWeight = 1f,  MinWave = 5 },
            new GateDefinition { Operation = GateOperation.Add,      OperandRange = (10, 30),SpawnWeight = 2f,  MinWave = 5 },
        ],

        PowerUpPalette =
        [
            new PowerUpDefinition { Type = PowerUpType.SpeedBoost,   Duration = 5f,  SpawnWeight = 1f   },
            new PowerUpDefinition { Type = PowerUpType.Shield,        Duration = 8f,  SpawnWeight = 1f   },
            new PowerUpDefinition { Type = PowerUpType.SlowEnemies,   Duration = 6f,  SpawnWeight = 0.8f },
            new PowerUpDefinition { Type = PowerUpType.ExtraSoldiers, Duration = 0f,  SpawnWeight = 0.6f,
                                    IsBlocked = true, BlockHealth = 4, OnShot = BlockHitBehavior.BreakConcrete,
                                    MinWave = 2 },
            new PowerUpDefinition { Type = PowerUpType.FreezeEnemies, Duration = 5f,  SpawnWeight = 0.5f, MinWave = 4 },
        ],

        // Wave ramp: enemy speed increases every wave; spawn rate tightens after wave 3
        Waves =
        [
            new WaveDefinition { WaveNumber = 2,  EnemySpeedMultiplier = 1.3f,  SpawnRateMultiplier = 0.95f },
            new WaveDefinition { WaveNumber = 3,  EnemySpeedMultiplier = 1.7f,  SpawnRateMultiplier = 0.85f },
            new WaveDefinition { WaveNumber = 4,  EnemySpeedMultiplier = 2.2f,  SpawnRateMultiplier = 0.70f },
            new WaveDefinition { WaveNumber = 5,  EnemySpeedMultiplier = 2.8f,  SpawnRateMultiplier = 0.55f, MaxEnemiesOverride = 500 },
            new WaveDefinition { WaveNumber = 6,  EnemySpeedMultiplier = 3.5f,  SpawnRateMultiplier = 0.40f, MaxEnemiesOverride = 650 },
            new WaveDefinition { WaveNumber = 7,  EnemySpeedMultiplier = 4.2f,  SpawnRateMultiplier = 0.30f, MaxEnemiesOverride = 800 },
            new WaveDefinition { WaveNumber = 8,  EnemySpeedMultiplier = 5.0f,  SpawnRateMultiplier = 0.22f, MaxEnemiesOverride = 800 },
        ]
    };
}
