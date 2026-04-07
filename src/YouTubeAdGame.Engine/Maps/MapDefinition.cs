namespace YouTubeAdGame.Engine.Maps;

/// <summary>
/// Top-level configuration for a game mode / map.
/// Each <see cref="Core.GameMode"/> maps to one <see cref="MapDefinition"/>
/// via <see cref="MapRegistry"/>.
/// </summary>
public sealed record MapDefinition
{
    // ── Identity ────────────────────────────────────────────────────────────
    /// <summary>Display name shown on the menu.</summary>
    public required string Name { get; init; }

    /// <summary>Short description shown on the menu.</summary>
    public required string Description { get; init; }

    // ── Road / lanes ────────────────────────────────────────────────────────
    /// <summary>Number of lanes on the road.</summary>
    public int LaneCount { get; init; } = 3;

    /// <summary>Width of a single lane in world units.</summary>
    public float LaneWidth { get; init; } = Core.GameConstants.LaneWidth;

    // ── Player tuning ───────────────────────────────────────────────────────
    /// <summary>Player horizontal speed (world-units / second).</summary>
    public float PlayerSpeed { get; init; } = Core.GameConstants.PlayerSpeed;

    /// <summary>Bullet speed (depth-units / second).</summary>
    public float BulletSpeed { get; init; } = Core.GameConstants.BulletSpeed;

    /// <summary>Seconds between automatic fire bursts.</summary>
    public float FireRate { get; init; } = Core.GameConstants.PlayerFireRate;

    // ── Enemy tuning ────────────────────────────────────────────────────────
    /// <summary>Base enemy speed (depth-units / second).</summary>
    public float EnemySpeed { get; init; } = Core.GameConstants.EnemySpeed;

    /// <summary>Maximum simultaneous enemies on screen.</summary>
    public int MaxEnemies { get; init; } = Core.GameConstants.MaxEnemiesOnScreen;

    /// <summary>Seconds between zombie spawn batches.</summary>
    public float SpawnInterval { get; init; } = Core.GameConstants.SpawnInterval;

    /// <summary>Number of zombies spawned per batch.</summary>
    public int ZombiesPerSpawn { get; init; } = Core.GameConstants.ZombiesPerSpawn;

    // ── Gate tuning ─────────────────────────────────────────────────────────
    /// <summary>Default gate scroll speed (depth-units / second).</summary>
    public float GateScrollSpeed { get; init; } = Core.GameConstants.GateScrollSpeed;

    /// <summary>Seconds between gate row spawns.</summary>
    public float GateSpawnInterval { get; init; } = Core.GameConstants.GateSpawnInterval;

    // ── Power-up tuning ─────────────────────────────────────────────────────
    /// <summary>Seconds between power-up spawns (0 = no power-ups).</summary>
    public float PowerUpSpawnInterval { get; init; }

    // ── Palettes ────────────────────────────────────────────────────────────
    /// <summary>Pool of gate types available in this map.</summary>
    public List<GateDefinition> GatePalette { get; init; } = [];

    /// <summary>Pool of power-up types available in this map.</summary>
    public List<PowerUpDefinition> PowerUpPalette { get; init; } = [];

    // ── Wave scripting ──────────────────────────────────────────────────────
    /// <summary>
    /// Per-wave overrides. Waves not listed here use the map defaults.
    /// </summary>
    public List<WaveDefinition> Waves { get; init; } = [];
}
