namespace YouTubeAdGame.Engine.Maps;

/// <summary>How a gate moves through the world.</summary>
public enum GateMovement
{
    /// <summary>Gate does not move — stays at its spawn position.</summary>
    Static,

    /// <summary>Gate scrolls with the world at the default scroll speed.</summary>
    ScrollWithWorld,

    /// <summary>Gate scrolls toward the player at a fast, independent speed.</summary>
    FastScroll,

    /// <summary>Gate oscillates horizontally within its lane.</summary>
    Oscillate
}
