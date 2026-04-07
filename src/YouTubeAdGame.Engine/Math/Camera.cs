using SkiaSharp;

namespace YouTubeAdGame.Engine.Math;

/// <summary>
/// Perspective camera that projects 3-D world coordinates onto a 2-D screen.
/// Uses a real pinhole-camera perspective (1/z falloff) rather than a linear
/// depth interpolation, so objects grow naturally fast as they approach the
/// camera, matching a genuine perspective projection.
///
/// World space
/// ───────────
///   X      : horizontal, −WorldHalfWidth … +WorldHalfWidth (0 = centre lane)
///   depth  : 0 (near / player) … MaxDepth (far / horizon)
///   height : 0 (road surface) … positive values (above road)
///
/// Screen space
/// ────────────
///   (0,0) at top-left, (Width, Height) at bottom-right.
///   Horizon line is at  HorizonY = HorizonFraction * Height.
///   Player line is at   GroundY  = GroundFraction  * Height.
/// </summary>
public readonly record struct Camera(float Width, float Height)
{
    public float HorizonY => Core.GameConstants.HorizonFraction * Height;
    public float GroundY  => Core.GameConstants.GroundFraction  * Height;
    public float CenterX  => Width * 0.5f;

    /// <summary>
    /// Perspective focal length.
    /// Chosen so scale = 1 at depth 0 and scale = FarScale at MaxDepth.
    /// f / (MaxDepth + f) = FarScale  ->  f = MaxDepth * FarScale / (1 - FarScale)
    /// </summary>
    private static float Focal =>
        Core.GameConstants.MaxDepth
        * Core.GameConstants.FarScale
        / (1f - Core.GameConstants.FarScale);   // ~87 with default constants

    /// <summary>
    /// Project a world position (and optional height above road) to screen space.
    /// Returns (screenX, screenY, scale) where scale drives object size.
    /// Uses real pinhole-camera perspective: scale = f / (depth + f).
    /// </summary>
    /// <param name="worldX">Horizontal world position (0 = centre lane).</param>
    /// <param name="depth">Depth from camera (0 = near/player, MaxDepth = horizon).</param>
    /// <param name="worldHeight">Height above road surface in world units (default 0).</param>
    public (float X, float Y, float Scale) Project(float worldX, float depth,
                                                    float worldHeight = 0f)
    {
        float d = System.Math.Max(depth, 0.001f);
        float f = Focal;

        // Real perspective scale: large near camera, tiny at the horizon
        float scale = f / (d + f);

        // Perspective X convergence toward vanishing point
        float screenX = CenterX
            + worldX * scale * (Width / (2f * Core.GameConstants.WorldHalfWidth));

        // Road-surface screen Y
        float groundScreenY = HorizonY + (GroundY - HorizonY) * scale;

        // Height above road lifts the object upward on screen (same perspective scale)
        float screenY = groundScreenY
            - worldHeight * scale * (Height / Core.GameConstants.MaxDepth) * 80f;

        return (screenX, screenY, scale);
    }

    /// <summary>
    /// Convert a screen-space X position back to a world-space X at the given depth.
    /// Used for touch/mouse input to world coordinates.
    /// </summary>
    public float ScreenToWorldX(float screenX, float depth)
    {
        float d  = System.Math.Max(depth, 0.001f);
        float scale = Focal / (d + Focal);
        return (screenX - CenterX) / (scale * (Width / (2f * Core.GameConstants.WorldHalfWidth)));
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
            (byte)(a.Alpha + (b.Alpha - a.Alpha)  * t)
        );
}
