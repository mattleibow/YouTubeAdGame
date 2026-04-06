using SkiaSharp;

namespace YouTubeAdGame.Engine.Math;

/// <summary>
/// Pseudo-3D camera that projects world coordinates onto a 2-D screen.
///
/// World space
/// ───────────
///   X  : horizontal, −WorldHalfWidth … +WorldHalfWidth (0 = centre lane)
///   depth : 0 (near / player) … MaxDepth (far / horizon)
///
/// Screen space
/// ────────────
///   (0,0) at top-left, (Width, Height) at bottom-right.
///   Horizon line is at  HorizonY = HorizonFraction * Height.
///   Player line is at   GroundY  = GroundFraction  * Height.
///
/// Objects get smaller and rise toward the horizon as depth increases.
/// </summary>
public readonly record struct Camera(float Width, float Height)
{
    public float HorizonY => Core.GameConstants.HorizonFraction * Height;
    public float GroundY  => Core.GameConstants.GroundFraction  * Height;
    public float CenterX  => Width * 0.5f;

    /// <summary>
    /// Project a world position to screen space.
    /// Returns (screenX, screenY, scale) where scale drives object size.
    /// </summary>
    public (float X, float Y, float Scale) Project(float worldX, float depth)
    {
        // Clamp depth so we never divide by zero or go behind camera
        float d = System.Math.Clamp(depth, 0f, Core.GameConstants.MaxDepth);

        // t = 0 → near (player), t = 1 → far (horizon)
        float t = d / Core.GameConstants.MaxDepth;

        // Linear interpolation along the vertical axis
        float screenY = HorizonY + (GroundY - HorizonY) * (1f - t);

        // Scale: large near the camera, tiny at the horizon
        float scale = MathHelper.Lerp(Core.GameConstants.NearScale, Core.GameConstants.FarScale, t);

        // Perspective X: world X converges toward the vanishing point
        float worldWidthAtDepth = Core.GameConstants.WorldHalfWidth * (1f - t * 0.92f);
        float screenHalfWidth   = (Width * 0.5f) * (1f - t * 0.08f);
        float screenX = CenterX + worldX * (screenHalfWidth / Core.GameConstants.WorldHalfWidth);

        return (screenX, screenY, scale);
    }

    /// <summary>
    /// Convert a screen-space X position back to a world-space X at the given depth.
    /// Used for touch/mouse input → world coordinates.
    /// </summary>
    public float ScreenToWorldX(float screenX, float depth)
    {
        float t = System.Math.Clamp(depth, 0f, Core.GameConstants.MaxDepth) / Core.GameConstants.MaxDepth;
        float screenHalfWidth = (Width * 0.5f) * (1f - t * 0.08f);
        return (screenX - CenterX) * Core.GameConstants.WorldHalfWidth / screenHalfWidth;
    }

    /// <summary>Depth fog alpha (0 = no fog, 1 = fully fogged).</summary>
    public static float FogAlpha(float depth)
    {
        float t = System.Math.Clamp(depth, 0f, Core.GameConstants.MaxDepth) / Core.GameConstants.MaxDepth;
        return System.MathF.Pow(t, 2.0f) * 0.7f;
    }
}

/// <summary>Common float math helpers.</summary>
public static class MathHelper
{
    public static float Lerp(float a, float b, float t) => a + (b - a) * t;
    public static float Clamp(float v, float min, float max) => v < min ? min : v > max ? max : v;
    public static float Clamp01(float v) => Clamp(v, 0f, 1f);

    public static SKColor LerpColor(SKColor a, SKColor b, float t) =>
        new(
            (byte)(a.Red   + (b.Red   - a.Red)   * t),
            (byte)(a.Green + (b.Green - a.Green) * t),
            (byte)(a.Blue  + (b.Blue  - a.Blue)  * t),
            (byte)(a.Alpha + (b.Alpha - a.Alpha) * t)
        );
}
