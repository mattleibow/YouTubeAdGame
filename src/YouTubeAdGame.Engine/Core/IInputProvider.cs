namespace YouTubeAdGame.Engine.Core;

/// <summary>
/// Snapshot of input state for a single frame.
/// Filled by the platform-specific input provider and consumed by the engine.
/// </summary>
public sealed class InputState
{
    /// <summary>Horizontal axis: −1 (left) … 0 … +1 (right). Normalised.</summary>
    public float HorizontalAxis { get; set; }

    /// <summary>True while the fire button / tap is held.</summary>
    public bool IsFiring { get; set; }

    /// <summary>True on the frame a tap/click begins (for UI interaction).</summary>
    public bool TapStarted { get; set; }

    /// <summary>Last pointer X in screen pixels.</summary>
    public float PointerX { get; set; }

    /// <summary>Last pointer Y in screen pixels.</summary>
    public float PointerY { get; set; }

    /// <summary>
    /// Copy values from another state (used to carry state across frames).
    /// </summary>
    public void CopyFrom(InputState other)
    {
        HorizontalAxis = other.HorizontalAxis;
        IsFiring = other.IsFiring;
        TapStarted = other.TapStarted;
        PointerX = other.PointerX;
        PointerY = other.PointerY;
    }

    /// <summary>Reset per-frame transient flags.</summary>
    public void EndFrame()
    {
        TapStarted = false;
    }
}

/// <summary>
/// Implemented by platform-specific input providers.
/// The engine calls <see cref="Poll"/> once per update tick.
/// </summary>
public interface IInputProvider
{
    /// <summary>Returns the current input state.</summary>
    InputState Poll();
}
