namespace YouTubeAdGame.Engine.Core;

/// <summary>Available game modes selectable from the main menu.</summary>
public enum GameMode
{
    /// <summary>
    /// Classic horde runner: 3-lane road, fast-scrolling gates, vast zombie hordes.
    /// Build your army through gates and survive as long as possible.
    /// </summary>
    HordeRunner,

    /// <summary>
    /// Lightning-fast variant: enemies are quicker, spawn rate is brutal, gates rush in rapidly.
    /// Think fast or die faster.
    /// </summary>
    Blitz,

    /// <summary>
    /// Starts gently but never stops ramping up. Enemies get faster every wave.
    /// Gates are rare and precious — every choice matters.
    /// </summary>
    Survival,

    /// <summary>
    /// Fully user-defined mode loaded from browser storage.
    /// Configure every parameter in the Map Editor.
    /// </summary>
    Custom
}
