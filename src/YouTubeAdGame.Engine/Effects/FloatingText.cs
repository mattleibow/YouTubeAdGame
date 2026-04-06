namespace YouTubeAdGame.Engine.Effects;

/// <summary>A short-lived text label that floats upward on the screen (e.g. "+20", "×3").</summary>
public sealed class FloatingText
{
    public string Text { get; init; } = "";

    /// <summary>Screen-space position.</summary>
    public float ScreenX { get; set; }
    public float ScreenY { get; set; }

    public float Lifetime { get; init; } = 1.2f;
    public float Elapsed  { get; set; }

    /// <summary>Fraction of life remaining (1 → 0).</summary>
    public float AlphaFraction => 1f - Elapsed / Lifetime;

    public bool IsExpired => Elapsed >= Lifetime;
}
