namespace YouTubeAdGame.Engine.Objects;

/// <summary>
/// Represents the group of soldiers marching with the player.
/// They are arranged in a circular blob formation whose radius grows
/// with the soldier count. Each member bobs and weaves slightly over time
/// to give the impression of a living, organic army.
///
/// Circle radius formula: radius = sqrt(Count) * BlobSpacingFactor
/// so that area scales linearly with count (1 soldier roughly = 1 unit of area).
/// </summary>
public sealed class Crowd
{
    private static readonly System.Random Rng = System.Random.Shared;

    // Stable per-member random seeds so each soldier has a consistent wobble identity
    private float[] _bobPhaseX  = [];
    private float[] _bobPhaseZ  = [];
    private float[] _memberNormX = [];
    private float[] _memberNormZ = [];

    private int _cachedCount = -1;

    /// <summary>Number of crowd members currently alive.</summary>
    public int Count { get; set; } = 1;

    /// <summary>Returns true when the crowd (and thus the run) is over.</summary>
    public bool IsEmpty => Count <= 0;

    /// <summary>Remove members, clamping to zero.</summary>
    public void Remove(int amount) => Count = System.Math.Max(0, Count - amount);

    /// <summary>
    /// Enumerate the world positions of visible crowd members for rendering and firing.
    /// Members are arranged in a circle blob around the player position.
    /// </summary>
    /// <param name="playerX">Player centre X in world units.</param>
    /// <param name="playerDepth">Player depth in world units.</param>
    /// <param name="time">Accumulated game time in seconds (drives the bobbing animation).</param>
    public System.Collections.Generic.IEnumerable<(float worldX, float depth)>
        GetMemberPositions(float playerX, float playerDepth, float time = 0f)
    {
        int visible = System.Math.Min(Count, Core.GameConstants.MaxCrowdVisible);
        EnsureSeeds(visible);

        float blobRadius = BlobRadius(Count);
        float amp = Core.GameConstants.SoldierBobAmplitude;

        for (int i = 0; i < visible; i++)
        {
            // Base position on a scaled circle (Poisson-like packing by sqrt)
            float nx = _memberNormX[i];
            float nz = _memberNormZ[i];
            float baseX = nx * blobRadius;
            float baseZ = nz * blobRadius;

            // Per-member sinusoidal bob/weave
            float wx = baseX + amp * System.MathF.Sin(time * 1.3f + _bobPhaseX[i]);
            float wz = baseZ + amp * System.MathF.Cos(time * 1.1f + _bobPhaseZ[i]);

            yield return (playerX + wx, playerDepth + wz);
        }
    }

    /// <summary>
    /// Blob radius scales so area is proportional to soldier count:
    /// r = spacing * sqrt(count), where spacing ≈ 1.05 gives ~1 soldier per unit area
    /// (tight hexagonal packing with ~1 unit gap between soldiers).
    /// </summary>
    public static float BlobRadius(int count) =>
        System.MathF.Sqrt(System.Math.Max(1, count)) * Core.GameConstants.CrowdMemberRadius * 1.05f;

    // ── Seed management ─────────────────────────────────────────────────────

    private void EnsureSeeds(int needed)
    {
        if (_cachedCount == needed) return;
        _cachedCount = needed;

        _bobPhaseX   = new float[needed];
        _bobPhaseZ   = new float[needed];
        _memberNormX = new float[needed];
        _memberNormZ = new float[needed];

        // Distribute members across concentric rings using a sunflower-seed pattern
        // (Vogel spiral) for even, gap-free coverage of the blob circle.
        const float goldenAngle = 2.39996322972865f; // radians, approx 137.5°

        for (int i = 0; i < needed; i++)
        {
            _bobPhaseX[i] = (float)(Rng.NextDouble() * System.Math.PI * 2.0);
            _bobPhaseZ[i] = (float)(Rng.NextDouble() * System.Math.PI * 2.0);

            // Sunflower spiral: radius proportional to sqrt(index) for uniform density
            float r     = (i == 0) ? 0f : System.MathF.Sqrt((float)i / needed);
            float angle = i * goldenAngle;
            _memberNormX[i] = r * System.MathF.Cos(angle);
            _memberNormZ[i] = r * System.MathF.Sin(angle) * 0.08f; // nearly flat — minimal depth spread
        }
    }
}
