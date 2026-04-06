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
/// Two gates appear side by side with different operations.
/// </summary>
public sealed class Gate : GameObjectBase
{
    public GateOperation Operation { get; init; }
    public int Operand { get; init; }

    /// <summary>Which side of the road this gate occupies.</summary>
    public bool IsLeftLane { get; init; }

    public Gate()
    {
        Radius = Core.GameConstants.GateWidth * 0.5f;
    }

    /// <summary>Human-readable label shown on the gate, e.g. "+20" or "×3".</summary>
    public string Label => Operation switch
    {
        GateOperation.Add        => $"+{Operand}",
        GateOperation.Subtract   => $"-{Operand}",
        GateOperation.Multiply   => $"×{Operand}",
        GateOperation.UpgradeGun => "⬆ GUN",
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
