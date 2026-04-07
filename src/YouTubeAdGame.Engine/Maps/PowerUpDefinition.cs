namespace YouTubeAdGame.Engine.Maps;

/// <summary>
/// Describes one kind of power-up that can appear in a map.
/// Power-ups may optionally be wrapped in a destructible concrete block.
/// </summary>
public sealed record PowerUpDefinition
{
    /// <summary>The effect type this power-up grants.</summary>
    public PowerUpType Type { get; init; }

    /// <summary>Duration of the effect in seconds (0 = instant, e.g. ExtraSoldiers).</summary>
    public float Duration { get; init; }

    /// <summary>Whether the power-up is wrapped in a concrete block that must be destroyed first.</summary>
    public bool IsBlocked { get; init; }

    /// <summary>Number of bullet hits required to break the concrete block.</summary>
    public int BlockHealth { get; init; } = 3;

    /// <summary>What happens when a bullet hits the concrete block.</summary>
    public BlockHitBehavior OnShot { get; init; } = BlockHitBehavior.BreakConcrete;

    /// <summary>For <see cref="BlockHitBehavior.IncrementCounter"/>: hits needed to break.</summary>
    public int CounterThreshold { get; init; } = 3;

    /// <summary>Relative probability when picking from the palette.</summary>
    public float SpawnWeight { get; init; } = 1f;

    /// <summary>Earliest wave this power-up can appear in.</summary>
    public int? MinWave { get; init; }

    /// <summary>Latest wave this power-up can appear in.</summary>
    public int? MaxWave { get; init; }
}
