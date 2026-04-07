namespace YouTubeAdGame.Engine.Maps;

/// <summary>What happens when a bullet hits a gate.</summary>
public enum GateHitBehavior
{
    /// <summary>Bullet passes through — gate is not affected.</summary>
    Nothing,

    /// <summary>Gate opens (becomes passable) after enough hits.</summary>
    Open,

    /// <summary>Gate closes (becomes blocked) when shot.</summary>
    Close,

    /// <summary>Gate toggles between open and closed on each hit.</summary>
    Toggle,

    /// <summary>Gate is destroyed (removed) after enough hits.</summary>
    Destroy,

    /// <summary>Each hit increments a visible counter; gate opens when counter reaches threshold.</summary>
    IncrementCounter
}
