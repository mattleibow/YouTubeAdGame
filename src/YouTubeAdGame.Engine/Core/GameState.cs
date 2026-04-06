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

    // ── Effects ─────────────────────────────────────────────────────────────
    public Effects.ScreenShake ScreenShake { get; } = new();
    public List<Effects.FloatingText> FloatingTexts { get; } = [];
    public List<Effects.Particle> Particles { get; } = [];

    // ── Spawn timers ────────────────────────────────────────────────────────
    public float EnemySpawnTimer { get; set; }
    public float GateSpawnTimer { get; set; }
    public float PlayerFireTimer { get; set; }

    // ── Viewport (set by renderer) ──────────────────────────────────────────
    public float ViewportWidth { get; set; } = 400f;
    public float ViewportHeight { get; set; } = 700f;
}
