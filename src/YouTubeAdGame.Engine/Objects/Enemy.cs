namespace YouTubeAdGame.Engine.Objects;

/// <summary>Standard enemy that moves toward the player.</summary>
public sealed class Enemy : GameObjectBase
{
    public Enemy()
    {
        Radius = Core.GameConstants.EnemyRadius;
    }

    public float Health { get; set; } = 1f;
    public float FireTimer { get; set; }

    /// <summary>Depth-units per second the enemy moves toward the player.</summary>
    public float Speed { get; set; } = Core.GameConstants.EnemySpeed;

    /// <summary>How much of the crowd this enemy removes on contact.</summary>
    public int CrowdDamage { get; set; } = 1;

    public bool IsAlive => Health > 0;
}

/// <summary>Boss enemy — large, high HP, special attacks.</summary>
public sealed class Boss : GameObjectBase
{
    public Boss()
    {
        Radius = Core.GameConstants.BossRadius;
    }

    public float Health { get; set; } = Core.GameConstants.BossHealth;
    public float MaxHealth { get; set; } = Core.GameConstants.BossHealth;
    public float FireTimer { get; set; }
    public float Speed { get; set; } = Core.GameConstants.BossSpeed;

    /// <summary>Fraction of crowd removed per special attack (0–1).</summary>
    public float CrowdDamageFraction { get; set; } = 0.25f;

    /// <summary>Cooldown for the special area attack.</summary>
    public float SpecialTimer { get; set; }

    public bool IsAlive => Health > 0;
}
