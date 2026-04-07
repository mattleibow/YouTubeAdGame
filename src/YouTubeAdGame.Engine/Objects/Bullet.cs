namespace YouTubeAdGame.Engine.Objects;

/// <summary>
/// Identifies who fired the bullet and determines its direction.
/// </summary>
public enum BulletOwner { Player, Enemy }

/// <summary>A projectile moving through world space.</summary>
public sealed class Bullet : GameObjectBase
{
    public BulletOwner Owner { get; init; }

    /// <summary>Forward speed in depth-units per second (positive = moving toward horizon).</summary>
    public float Speed { get; init; }

    /// <summary>
    /// Horizontal velocity in world-X units per second.
    /// Non-zero when the bullet is aimed at a specific target rather than firing straight ahead.
    /// </summary>
    public float VelocityX { get; set; }

    public Bullet(BulletOwner owner, float worldX, float depth, float speed,
                  float velocityX = 0f)
    {
        Owner     = owner;
        WorldX    = worldX;
        Depth     = depth;
        Speed     = speed;
        VelocityX = velocityX;
        Radius    = 6f;
    }
}

