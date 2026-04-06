using SkiaSharp;

namespace YouTubeAdGame.Engine.Core;

/// <summary>
/// Rendering abstraction — implemented by platform-specific renderers
/// (SkiaSharp for Blazor WASM, MAUI, or desktop).
/// </summary>
public interface IRenderer
{
    /// <summary>Width of the drawing surface in pixels.</summary>
    float Width { get; }

    /// <summary>Height of the drawing surface in pixels.</summary>
    float Height { get; }

    /// <summary>Render the complete game frame from the provided state.</summary>
    void Render(SKCanvas canvas, GameState state);
}
