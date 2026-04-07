namespace YouTubeAdGame.Engine.Maps;

/// <summary>
/// Inclusive integer range used to randomize a gate's operand at spawn time.
/// Replaces the raw value-tuple so the type is JSON-serializable.
/// </summary>
public sealed record OperandRange(int Min, int Max)
{
    /// <summary>Implicit conversion from a value-tuple so existing initializer syntax still compiles.</summary>
    public static implicit operator OperandRange((int Min, int Max) t) => new(t.Min, t.Max);
}
