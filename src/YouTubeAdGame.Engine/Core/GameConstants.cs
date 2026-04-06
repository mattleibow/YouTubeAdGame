namespace YouTubeAdGame.Engine.Core;

/// <summary>Game-wide constants and tuning values.</summary>
public static class GameConstants
{
    // World dimensions (logical units)
    public const float WorldHalfWidth = 300f;
    public const float MaxDepth = 1000f;
    public const float PlayerDepth = 60f;   // depth at which the player sits

    // Pseudo-3D camera fractions (0–1 of viewport height)
    public const float HorizonFraction = 0.32f;   // where the road vanishes
    public const float GroundFraction = 0.95f;    // bottom of the visible road

    // Scale at different depths
    public const float NearScale = 1.0f;
    public const float FarScale = 0.08f;

    // Player
    public const float PlayerRadius = 28f;
    public const float PlayerSpeed = 220f;   // world-units / second
    public const float PlayerFireRate = 0.22f; // seconds between bullets
    public const float BulletSpeed = 800f;

    // Enemy
    public const float EnemyRadius = 22f;
    public const float EnemySpeed = 120f;   // depth-units / second
    public const float EnemyFireRate = 2.0f;
    public const float EnemyBulletSpeed = 300f;

    // Boss
    public const float BossRadius = 55f;
    public const float BossHealth = 20f;
    public const float BossSpeed = 60f;

    // Crowd
    public const float CrowdMemberRadius = 14f;
    public const float CrowdSpacingX = 38f;
    public const float CrowdSpacingDepth = 32f;
    public const int CrowdColumns = 5;
    public const int MaxCrowdVisible = 25;

    // Gate
    public const float GateWidth = 100f;
    public const float GateHeight = 60f;
    public const float GateDepth = 20f;     // world-depth size

    // Game loop
    public const double TargetFps = 60.0;
    public const double FixedDeltaTime = 1.0 / TargetFps;

    // Spawn
    public const float SpawnDepth = MaxDepth - 20f;
    public const float SpawnInterval = 2.5f;   // seconds between enemy waves
    public const float GateSpawnInterval = 4.0f;
}
