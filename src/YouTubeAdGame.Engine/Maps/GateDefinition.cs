using YouTubeAdGame.Engine.Objects;

namespace YouTubeAdGame.Engine.Maps;

/// <summary>
/// Describes one kind of gate that can appear in a map.
/// The <see cref="Maps.MapDefinition.GatePalette"/> contains a pool of these;
/// the spawn system picks from them based on weight and wave filters.
/// </summary>
public sealed record GateDefinition
{
    /// <summary>The math operation this gate applies to the crowd.</summary>
    public GateOperation Operation { get; init; }

    /// <summary>
    /// Inclusive range for the randomized operand.
    /// A random value in [Min, Max] is chosen at spawn time.
    /// </summary>
    public (int Min, int Max) OperandRange { get; init; }

    /// <summary>How the gate moves through the world.</summary>
    public GateMovement MovementStyle { get; init; } = GateMovement.FastScroll;

    /// <summary>
    /// Override scroll speed in depth-units/sec.
    /// <c>null</c> = use <see cref="MapDefinition.GateScrollSpeed"/>.
    /// </summary>
    public float? ScrollSpeed { get; init; }

    /// <summary>Whether the gate starts open (passable) or closed (blocked).</summary>
    public bool IsOpen { get; init; } = true;

    /// <summary>What happens when a bullet hits this gate.</summary>
    public GateHitBehavior OnShot { get; init; } = GateHitBehavior.Nothing;

    /// <summary>Number of bullet hits required to trigger <see cref="OnShot"/> (for Open/Destroy/Counter).</summary>
    public int HitsToOpen { get; init; } = 1;

    /// <summary>Relative probability when picking from the palette (higher = more likely).</summary>
    public float SpawnWeight { get; init; } = 1f;

    /// <summary>Earliest wave this gate type can appear in (<c>null</c> = no minimum).</summary>
    public int? MinWave { get; init; }

    /// <summary>Latest wave this gate type can appear in (<c>null</c> = no maximum).</summary>
    public int? MaxWave { get; init; }

    /// <summary>Whether this gate type may only appear in the left lane (e.g. gun upgrades).</summary>
    public bool LeftLaneOnly { get; init; }
}
