namespace YouTubeAdGame.Engine.Objects;

/// <summary>
/// A solid wall that scrolls toward the player.
/// Bullets chip away at its health; if it reaches the crowd it crushes soldiers.
/// </summary>
public sealed class Barrier : GameObjectBase
{
    /// <summary>Current hit points.</summary>
    public int Health { get; set; }

    /// <summary>Starting hit points (used to draw the health bar).</summary>
    public int MaxHealth { get; set; }

    /// <summary>Width of the barrier in world units.</summary>
    public float Width  { get; set; } = 180f;

    /// <summary>Visual height of the barrier in world units (treated like GateHeight).</summary>
    public float Height { get; set; } = 50f;

    /// <summary>Soldiers lost when this barrier reaches the crowd.</summary>
    public int CrowdDamage { get; set; } = 3;

    /// <summary>Scroll speed (depth-units per second). Defaults to GateScrollSpeed.</summary>
    public float ScrollSpeed { get; set; }
}
