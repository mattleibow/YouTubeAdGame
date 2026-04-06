using YouTubeAdGame.Engine.Core;

namespace YouTubeAdGame.Web.Input;

/// <summary>
/// Blazor-specific input provider.
/// Tracks mouse, touch, and keyboard input via JavaScript events hoisted from the canvas element.
/// The game canvas component updates this object's mutable state; the engine polls it each tick.
/// </summary>
public sealed class BlazorInputProvider : IInputProvider
{
    private readonly InputState _state = new();

    // ── Called from GameCanvas on JS events ────────────────────────────────

    /// <summary>Pointer moved to (x, y) on the canvas in CSS pixels.</summary>
    public void OnPointerMove(float x, float y, float canvasWidth)
    {
        _state.PointerX = x;
        _state.PointerY = y;
        // Derive axis from pointer X vs centre
        float centre = canvasWidth * 0.5f;
        _state.HorizontalAxis = System.Math.Clamp((x - centre) / (centre * 0.6f), -1f, 1f);
        _state.IsFiring = true;   // holding pointer = firing
    }

    public void OnPointerDown(float x, float y, float canvasWidth)
    {
        _state.TapStarted = true;
        OnPointerMove(x, y, canvasWidth);
    }

    public void OnPointerUp()
    {
        _state.IsFiring = false;
        _state.HorizontalAxis = 0f;
    }

    public void OnKeyDown(string key)
    {
        switch (key)
        {
            case "ArrowLeft" or "a" or "A":
                _state.HorizontalAxis = -1f;
                _state.IsFiring = true;
                break;
            case "ArrowRight" or "d" or "D":
                _state.HorizontalAxis = 1f;
                _state.IsFiring = true;
                break;
            case " " or "Enter":
                _state.TapStarted = true;
                break;
        }
    }

    public void OnKeyUp(string key)
    {
        switch (key)
        {
            case "ArrowLeft" or "a" or "A" or "ArrowRight" or "d" or "D":
                _state.HorizontalAxis = 0f;
                _state.IsFiring = false;
                break;
        }
    }

    // ── IInputProvider ────────────────────────────────────────────────────

    public InputState Poll()
    {
        // Return a copy of the current state so the engine can't mutate it
        var snapshot = new InputState();
        snapshot.CopyFrom(_state);
        // Clear transient flags
        _state.EndFrame();
        return snapshot;
    }
}
