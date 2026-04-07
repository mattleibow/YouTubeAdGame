namespace YouTubeAdGame.Engine.Maps;

/// <summary>
/// Per-wave tuning overrides. When a wave's number matches <see cref="WaveNumber"/>,
/// these multipliers/overrides are applied on top of the map defaults.
/// </summary>
public sealed record WaveDefinition
{
    /// <summary>Which wave this definition applies to.</summary>
    public int WaveNumber { get; init; }

    /// <summary>Multiplier applied to the map's base enemy speed (1.0 = default).</summary>
    public float EnemySpeedMultiplier { get; init; } = 1f;

    /// <summary>Multiplier applied to the spawn interval (smaller = faster spawns).</summary>
    public float SpawnRateMultiplier { get; init; } = 1f;

    /// <summary>Override for the maximum simultaneous enemies (<c>null</c> = use map default).</summary>
    public int? MaxEnemiesOverride { get; init; }

    /// <summary>Override for the gate spawn interval (<c>null</c> = use map default).</summary>
    public float? GateIntervalOverride { get; init; }

    /// <summary>
    /// Indices into the map's <see cref="MapDefinition.GatePalette"/> that are available this wave.
    /// <c>null</c> or empty = all gates available.
    /// </summary>
    public List<int>? AvailableGates { get; init; }

    /// <summary>
    /// Indices into the map's <see cref="MapDefinition.PowerUpPalette"/> that are available this wave.
    /// <c>null</c> or empty = all power-ups available.
    /// </summary>
    public List<int>? AvailablePowerUps { get; init; }
}
