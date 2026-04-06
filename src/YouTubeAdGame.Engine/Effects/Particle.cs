namespace YouTubeAdGame.Engine.Effects;

/// <summary>A single particle emitted when an enemy dies.</summary>
public sealed class Particle
{
    public float ScreenX   { get; set; }
    public float ScreenY   { get; set; }
    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
    public float Radius    { get; set; } = 5f;
    public float Lifetime  { get; set; } = 0.6f;
    public float Elapsed   { get; set; }

    public float AlphaFraction => 1f - Elapsed / Lifetime;
    public bool IsExpired => Elapsed >= Lifetime;

    /// <summary>
    /// Spawn a burst of particles at the given screen position.
    /// </summary>
    public static IEnumerable<Particle> Burst(float screenX, float screenY, int count = 8)
    {
        var rng = new Random();
        for (int i = 0; i < count; i++)
        {
            double angle = rng.NextDouble() * System.Math.PI * 2;
            float speed  = 60f + (float)rng.NextDouble() * 100f;
            yield return new Particle
            {
                ScreenX    = screenX,
                ScreenY    = screenY,
                VelocityX  = (float)System.Math.Cos(angle) * speed,
                VelocityY  = (float)System.Math.Sin(angle) * speed,
                Radius     = 3f + (float)rng.NextDouble() * 5f,
                Lifetime   = 0.4f + (float)rng.NextDouble() * 0.4f
            };
        }
    }
}
