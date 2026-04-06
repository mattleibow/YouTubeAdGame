namespace YouTubeAdGame.Engine.Core;

/// <summary>Game-wide constants and tuning values.</summary>
public static class GameConstants
{
    // World dimensions (logical units)
    public const float WorldHalfWidth = 300f;
    public const float MaxDepth = 1000f;
    public const float PlayerDepth = 150f;  // depth at which the player sits

    // Pseudo-3D camera fractions (0–1 of viewport height)
    public const float HorizonFraction = 0.32f;   // where the road vanishes
    public const float GroundFraction = 0.95f;    // bottom of the visible road

    // Scale at different depths
    public const float NearScale = 1.0f;
    public const float FarScale = 0.08f;

    // Player
    public const float PlayerRadius = 18f;
    public const float PlayerSpeed = 350f;   // world-units / second
    public const float PlayerFireRate = 0.18f; // seconds between bullets
    public const float BulletSpeed = 900f;

    // Enemy
    public const float EnemyRadius = 14f;
    public const float EnemySpeed = 90f;    // depth-units / second
    public const float EnemyFireRate = 3.0f;
    public const float EnemyBulletSpeed = 300f;

    // Boss
    public const float BossRadius = 55f;
    public const float BossHealth = 20f;
    public const float BossSpeed = 60f;

    // Crowd
    public const float CrowdMemberRadius = 9f;
    public const float CrowdSpacingX = 14f;
    public const float CrowdSpacingDepth = 9f;
    public const int CrowdColumns = 10;
    public const int MaxCrowdVisible = 300;
    /// <summary>Half the width of the full crowd formation (keeps crowd on road).</summary>
    public const float CrowdHalfWidth = CrowdSpacingX * CrowdColumns / 2f;

    // Gate
    public const float GateWidth = 100f;
    public const float GateHeight = 60f;
    public const float GateDepth = 20f;     // world-depth size

    // Game loop
    public const double TargetFps = 60.0;
    public const double FixedDeltaTime = 1.0 / TargetFps;

    // Spawn
    public const float SpawnDepth = MaxDepth - 20f;
    public const float SpawnInterval = 0.1f;   // seconds between zombie spawn batches (100 ms)
    public const int ZombiesPerSpawn = 8;       // zombies added per batch
    public const float GateSpawnInterval = 4.0f;
    public const int MaxEnemiesOnScreen = 500;  // cap on simultaneous zombies
}
