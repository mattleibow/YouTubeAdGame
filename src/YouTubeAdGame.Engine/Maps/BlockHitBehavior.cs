namespace YouTubeAdGame.Engine.Maps;

/// <summary>What happens when a bullet hits a blocked (concrete-wrapped) power-up.</summary>
public enum BlockHitBehavior
{
    /// <summary>Bullet has no effect on the block.</summary>
    Nothing,

    /// <summary>Each hit chips away at the concrete until it breaks.</summary>
    BreakConcrete,

    /// <summary>Each hit increments a visible counter; block breaks when counter reaches threshold.</summary>
    IncrementCounter
}
