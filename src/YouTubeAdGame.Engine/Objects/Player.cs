namespace YouTubeAdGame.Engine.Objects;

/// <summary>The player-controlled character at the bottom of the screen.</summary>
public sealed class Player : GameObjectBase
{
    public Player()
    {
        Depth  = Core.GameConstants.PlayerDepth;
        Radius = Core.GameConstants.PlayerRadius;
    }

    /// <summary>Velocity used for smooth deceleration.</summary>
    public float VelocityX { get; set; }

    /// <summary>Health: when zero the game ends.</summary>
    public int Health { get; set; } = 3;

    /// <summary>Flash timer after taking a hit (seconds).</summary>
    public float HitFlashTimer { get; set; }

    /// <summary>Current gun upgrade level (0 = single shot, 1 = spread, etc.).</summary>
    public int GunLevel { get; set; }

    public bool IsAlive => Health > 0;
}
