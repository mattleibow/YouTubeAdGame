using YouTubeAdGame.Engine.Maps;

namespace YouTubeAdGame.Engine.Objects;

/// <summary>
/// A collectable power-up in world space. May optionally be wrapped
/// in a destructible concrete block (<see cref="IsBlocked"/>).
/// </summary>
public sealed class PowerUp : GameObjectBase
{
    /// <summary>The effect this power-up grants on collection.</summary>
    public PowerUpType Type { get; init; }

    /// <summary>Duration of the effect in seconds (0 = instant).</summary>
    public float Duration { get; init; }

    /// <summary>Whether the power-up is currently wrapped in a concrete block.</summary>
    public bool IsBlocked { get; set; }

    /// <summary>Remaining bullet hits needed to break the concrete.</summary>
    public int BlockHitsRemaining { get; set; }

    /// <summary>What happens when a bullet hits the block.</summary>
    public BlockHitBehavior OnShot { get; init; } = BlockHitBehavior.BreakConcrete;

    /// <summary>
    /// For <see cref="BlockHitBehavior.IncrementCounter"/>: current hit count shown on the block.
    /// </summary>
    public int HitCounter { get; set; }

    /// <summary>For counter-based unlocking: target count to break.</summary>
    public int CounterThreshold { get; init; }

    /// <summary>True when the concrete is broken and the power-up can be collected.</summary>
    public bool IsRevealed => !IsBlocked;

    public PowerUp()
    {
        Radius = Core.GameConstants.GateCollisionRadius * 0.6f;  // slightly smaller than gates
    }

    /// <summary>Accumulated bob time (seconds). Drives the floating animation.</summary>
    public float BobTimer { get; set; }

    /// <summary>Human-readable label for the power-up icon.</summary>
    public string Label => Type switch
    {
        PowerUpType.SpeedBoost    => "SPEED",
        PowerUpType.Shield        => "SHIELD",
        PowerUpType.RapidFire     => "RAPID",
        PowerUpType.BulletPierce  => "PIERCE",
        PowerUpType.ExtraSoldiers => "+5",
        PowerUpType.GunUpgrade    => "GUN UP",
        PowerUpType.SlowEnemies   => "SLOW",
        PowerUpType.FreezeEnemies => "FREEZE",
        _                         => "?"
    };
}
