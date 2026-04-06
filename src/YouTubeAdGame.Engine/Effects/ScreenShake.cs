namespace YouTubeAdGame.Engine.Effects;

/// <summary>Screen-shake effect driven by a trauma value.</summary>
public sealed class ScreenShake
{
    private float _trauma;
    private float _offsetX;
    private float _offsetY;
    private readonly Random _rng = new();

    /// <summary>Add trauma (0–1). Values are clamped and decay over time.</summary>
    public void AddTrauma(float amount) =>
        _trauma = System.Math.Clamp(_trauma + amount, 0f, 1f);

    /// <summary>Current horizontal pixel offset (apply to canvas translation).</summary>
    public float OffsetX => _offsetX;

    /// <summary>Current vertical pixel offset.</summary>
    public float OffsetY => _offsetY;

    /// <summary>True while shake is active.</summary>
    public bool IsActive => _trauma > 0.001f;

    /// <summary>Call once per frame to advance the shake simulation.</summary>
    public void Update(float dt)
    {
        if (_trauma <= 0f)
        {
            _offsetX = 0f;
            _offsetY = 0f;
            return;
        }

        float shake = _trauma * _trauma;
        const float maxOffset = 16f;
        _offsetX = maxOffset * shake * (float)(_rng.NextDouble() * 2.0 - 1.0);
        _offsetY = maxOffset * shake * (float)(_rng.NextDouble() * 2.0 - 1.0);

        _trauma -= dt * 2.5f;  // decay rate
        if (_trauma < 0f) _trauma = 0f;
    }
}
