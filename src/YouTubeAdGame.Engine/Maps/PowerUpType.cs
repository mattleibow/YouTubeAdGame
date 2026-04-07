namespace YouTubeAdGame.Engine.Maps;

/// <summary>Types of power-up effects the player can collect.</summary>
public enum PowerUpType
{
    /// <summary>Temporarily increases player movement speed.</summary>
    SpeedBoost,

    /// <summary>Temporary shield that absorbs one zombie hit.</summary>
    Shield,

    /// <summary>Temporarily increases fire rate.</summary>
    RapidFire,

    /// <summary>Bullets pierce through multiple enemies.</summary>
    BulletPierce,

    /// <summary>Instantly adds extra soldiers to the crowd.</summary>
    ExtraSoldiers,

    /// <summary>Upgrades the gun level (same as gun gate).</summary>
    GunUpgrade,

    /// <summary>Temporarily slows all enemies.</summary>
    SlowEnemies,

    /// <summary>Temporarily freezes all enemies in place.</summary>
    FreezeEnemies
}
