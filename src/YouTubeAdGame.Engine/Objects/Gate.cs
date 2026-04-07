using YouTubeAdGame.Engine.Maps;

namespace YouTubeAdGame.Engine.Objects;

/// <summary>Operations a gate can apply to the crowd count.</summary>
public enum GateOperation
{
    Add,
    Subtract,
    Multiply,
    UpgradeGun
}

/// <summary>
/// A gate the player can walk through to modify the crowd count.
/// Gates spawn across lanes and may be open (passable) or closed (blocked).
/// </summary>
public sealed class Gate : GameObjectBase
{
    public GateOperation Operation { get; init; }
    public int Operand { get; init; }

    /// <summary>Which side of the road this gate occupies.</summary>
    public bool IsLeftLane { get; init; }

    // ── Data-driven behaviour ───────────────────────────────────────────────

    /// <summary>How the gate moves through the world.</summary>
    public GateMovement Movement { get; init; } = GateMovement.FastScroll;

    /// <summary>Scroll speed override (depth-units / sec). 0 = use map default.</summary>
    public float ScrollSpeed { get; init; }

    /// <summary>Whether the gate is currently passable.</summary>
    public bool IsOpen { get; set; } = true;

    /// <summary>What happens when a bullet hits this gate.</summary>
    public GateHitBehavior OnShot { get; init; } = GateHitBehavior.Nothing;

    /// <summary>Remaining bullet hits before <see cref="OnShot"/> triggers (for Open/Destroy/Counter).</summary>
    public int HitsRemaining { get; set; }

    /// <summary>
    /// Visible counter for <see cref="GateHitBehavior.IncrementCounter"/> gates.
    /// </summary>
    public int HitCounter { get; set; }

    /// <summary>Timer used for <see cref="GateMovement.Oscillate"/> movement.</summary>
    public float OscillateTimer { get; set; }

    /// <summary>Centre X of the lane this gate was spawned in (used for oscillation).</summary>
    public float LaneCenterX { get; init; }

    public Gate()
    {
        Radius = Core.GameConstants.GateCollisionRadius;
    }

    /// <summary>Human-readable label shown on the gate, e.g. "+20" or "×3".</summary>
    public string Label => Operation switch
    {
        GateOperation.Add        => $"+{Operand}",
        GateOperation.Subtract   => $"-{Operand}",
        GateOperation.Multiply   => $"×{Operand}",
        GateOperation.UpgradeGun => "GUN UP",
        _                        => "?"
    };

    /// <summary>Apply this gate's effect to the game state.</summary>
    public void Apply(Core.GameState state)
    {
        switch (Operation)
        {
            case GateOperation.Add:
                state.Crowd.Count += Operand;
                break;
            case GateOperation.Subtract:
                state.Crowd.Count = System.Math.Max(0, state.Crowd.Count - Operand);
                break;
            case GateOperation.Multiply:
                state.Crowd.Count *= Operand;
                break;
            case GateOperation.UpgradeGun:
                state.Player.GunLevel++;
                break;
        }
    }
}
