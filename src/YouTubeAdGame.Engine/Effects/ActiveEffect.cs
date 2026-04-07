using YouTubeAdGame.Engine.Maps;

namespace YouTubeAdGame.Engine.Effects;

/// <summary>
/// A time-limited effect currently active on the player (e.g. shield, rapid fire).
/// Stored in <see cref="Core.GameState.ActiveEffects"/>.
/// </summary>
public sealed class ActiveEffect
{
    /// <summary>Which power-up type is active.</summary>
    public PowerUpType Type { get; init; }

    /// <summary>Total duration of this effect in seconds.</summary>
    public float Duration { get; init; }

    /// <summary>Seconds elapsed since activation.</summary>
    public float Elapsed { get; set; }

    /// <summary>Fraction of time remaining (1 → 0).</summary>
    public float RemainingFraction => 1f - Elapsed / Duration;

    /// <summary>True once the effect has expired.</summary>
    public bool IsExpired => Duration > 0f && Elapsed >= Duration;
}
