namespace YouTubeAdGame.Engine.Objects;

/// <summary>
/// Represents the group of followers trailing the player.
/// The actual count drives the rendering of crowd members in a grid behind the player.
/// </summary>
public sealed class Crowd
{
    /// <summary>Number of crowd members currently alive.</summary>
    public int Count { get; set; } = 10;

    /// <summary>Returns true when the crowd (and thus the run) is over.</summary>
    public bool IsEmpty => Count <= 0;

    /// <summary>Remove members, clamping to zero.</summary>
    public void Remove(int amount) => Count = System.Math.Max(0, Count - amount);

    /// <summary>
    /// Enumerate the world positions of visible crowd members for rendering.
    /// Members are arranged in a grid behind the player.
    /// </summary>
    public IEnumerable<(float worldX, float depth)> GetMemberPositions(float playerX, float playerDepth)
    {
        int visible = System.Math.Min(Count, Core.GameConstants.MaxCrowdVisible);
        int cols    = Core.GameConstants.CrowdColumns;

        for (int i = 0; i < visible; i++)
        {
            int row = i / cols;
            int col = i % cols;

            float offsetX     = (col - cols / 2) * Core.GameConstants.CrowdSpacingX;
            float offsetDepth = -(row + 1) * Core.GameConstants.CrowdSpacingDepth;

            yield return (
                playerX + offsetX,
                playerDepth + offsetDepth
            );
        }
    }
}
