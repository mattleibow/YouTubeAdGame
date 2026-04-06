namespace YouTubeAdGame.Engine.Objects;

/// <summary>
/// Identifies who fired the bullet and determines its direction.
/// </summary>
public enum BulletOwner { Player, Enemy }

/// <summary>A projectile moving through world space.</summary>
public sealed class Bullet : GameObjectBase
{
    public BulletOwner Owner { get; init; }

    /// <summary>Speed in depth-units per second (negative = moving toward player).</summary>
    public float Speed { get; init; }

    public Bullet(BulletOwner owner, float worldX, float depth, float speed)
    {
        Owner   = owner;
        WorldX  = worldX;
        Depth   = depth;
        Speed   = speed;
        Radius  = 6f;
    }
}
