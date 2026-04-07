namespace YouTubeAdGame.Engine.Core;

/// <summary>Overall game state machine.</summary>
public enum GamePhase
{
    Menu,
    Playing,
    Paused,
    GameOver,
    Victory
}

/// <summary>
/// Holds all mutable runtime data for a game session.
/// Passed to the engine each update/render tick.
/// </summary>
public sealed class GameState
{
    // ── Phase ───────────────────────────────────────────────────────────────
    public GamePhase Phase { get; set; } = GamePhase.Menu;

    // ── Mode ────────────────────────────────────────────────────────────────
    /// <summary>The game mode selected from the menu.</summary>
    public GameMode Mode { get; set; } = GameMode.HordeRunner;

    // ── Active map ──────────────────────────────────────────────────────────
    /// <summary>The currently loaded map definition for the active mode.</summary>
    public Maps.MapDefinition? ActiveMap { get; set; }

    // ── Score / progression ─────────────────────────────────────────────────
    public int Score { get; set; }
    public int Wave { get; set; }
    public float Distance { get; set; }  // total world-units scrolled

    // ── World scroll ────────────────────────────────────────────────────────
    /// <summary>Global scroll offset (added to every object's depth each frame).</summary>
    public float ScrollOffset { get; set; }

    // ── Objects ─────────────────────────────────────────────────────────────
    public Objects.Player Player { get; } = new();
    public Objects.Crowd Crowd { get; } = new();
    public List<Objects.Enemy> Enemies { get; } = [];
    public List<Objects.Bullet> PlayerBullets { get; } = [];
    public List<Objects.Bullet> EnemyBullets { get; } = [];
    public List<Objects.Gate> Gates { get; } = [];
    public List<Objects.Obstacle> Obstacles { get; } = [];
    public List<Objects.PowerUp> PowerUps { get; } = [];

    // ── Effects ─────────────────────────────────────────────────────────────
    public Effects.ScreenShake ScreenShake { get; } = new();
    public List<Effects.FloatingText> FloatingTexts { get; } = [];
    public List<Effects.Particle> Particles { get; } = [];

    /// <summary>Time-limited effects currently active on the player.</summary>
    public List<Effects.ActiveEffect> ActiveEffects { get; } = [];

    // ── Spawn timers ────────────────────────────────────────────────────────
    public float EnemySpawnTimer { get; set; }
    public float GateSpawnTimer { get; set; }
    public float PlayerFireTimer { get; set; }

    /// <summary>Timer for power-up spawning.</summary>
    public float PowerUpSpawnTimer { get; set; }

    // ── Runtime tuning (adjustable via debug inspector) ──────────────────────
    /// <summary>Maximum simultaneous zombies on screen. Adjustable at runtime.</summary>
    public int MaxEnemiesOnScreen { get; set; } = GameConstants.MaxEnemiesOnScreen;

    /// <summary>Seconds between zombie spawn batches. Adjustable at runtime.</summary>
    public float SpawnInterval { get; set; } = GameConstants.SpawnInterval;

    // ── Viewport (set by renderer) ──────────────────────────────────────────
    public float ViewportWidth { get; set; } = 400f;
    public float ViewportHeight { get; set; } = 700f;
}
