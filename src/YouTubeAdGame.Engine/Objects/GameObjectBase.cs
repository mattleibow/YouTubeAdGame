namespace YouTubeAdGame.Engine.Objects;

/// <summary>
/// Base for every object that lives in world space.
/// </summary>
public abstract class GameObjectBase
{
    /// <summary>Horizontal position in world units (0 = centre lane).</summary>
    public float WorldX { get; set; }

    /// <summary>
    /// Depth in world units.
    /// 0 = at the player's feet; <see cref="Core.GameConstants.MaxDepth"/> = horizon.
    /// </summary>
    public float Depth { get; set; }

    /// <summary>Logical radius for circle-based collision checks.</summary>
    public float Radius { get; set; }

    /// <summary>Whether this object should be removed from the world.</summary>
    public bool IsDestroyed { get; set; }

    /// <summary>
    /// Simple AABB / circle collision check against another object.
    /// Uses the projected screen size at depth for a rough-but-effective check.
    /// </summary>
    public bool Overlaps(GameObjectBase other)
    {
        float dx = WorldX - other.WorldX;
        float dz = Depth  - other.Depth;
        float minDist = Radius + other.Radius;
        return dx * dx + dz * dz < minDist * minDist;
    }
}
