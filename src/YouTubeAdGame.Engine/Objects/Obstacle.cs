namespace YouTubeAdGame.Engine.Objects;

/// <summary>A static road obstacle the player/crowd must navigate around.</summary>
public sealed class Obstacle : GameObjectBase
{
    public float Width  { get; set; } = 40f;
    public float Height { get; set; } = 40f;

    public Obstacle()
    {
        Radius = 20f;
    }
}
