using SkiaSharp;
using YouTubeAdGame.Engine.Core;
using YouTubeAdGame.Engine.Maps;
using YouTubeAdGame.Engine.Math;
using YouTubeAdGame.Engine.Objects;
using YouTubeAdGame.Engine.Effects;

namespace YouTubeAdGame.Engine.Rendering;

/// <summary>
/// Full SkiaSharp implementation of <see cref="IRenderer"/>.
/// Draws the pseudo-3D top-down shooter scene frame by frame.
///
/// Rendering order (back to front):
///  1. Background / sky
///  2. Road (perspective trapezoid)
///  3. Road markings
///  4. Obstacles
///  5. Gates
///  6. Enemies (sorted by depth, far → near)
///  7. Enemy bullets
///  8. Player bullets
///  9. Crowd members
/// 10. Player
/// 11. Effects (particles, floating text)
/// 12. HUD overlay
/// </summary>
public sealed class SkiaGameRenderer : IRenderer
{
    // ── Palette ─────────────────────────────────────────────────────────────
    private static readonly SKColor ColSky        = new(0xFF_1A1A2E);
    private static readonly SKColor ColHorizon    = new(0xFF_16213E);
    private static readonly SKColor ColRoadNear   = new(0xFF_2D3561);
    private static readonly SKColor ColRoadFar    = new(0xFF_1C2340);
    private static readonly SKColor ColLane       = new(0xFF_FFD700);
    private static readonly SKColor ColPlayer     = new(0xFF_00E5FF);
    private static readonly SKColor ColPlayerFlash= new(0xFF_FF2D55);
    private static readonly SKColor ColCrowd      = new(0xFF_00B4D8);
    private static readonly SKColor ColEnemy      = new(0xFF_FF3B30);
    private static readonly SKColor ColEnemyShadow= new(0x55_000000);
    private static readonly SKColor ColBulletP    = new(0xFF_FFD60A);
    private static readonly SKColor ColBulletE    = new(0xFF_FF375F);
    private static readonly SKColor ColGateAdd    = new(0xFF_30D158);
    private static readonly SKColor ColGateSub    = new(0xFF_FF453A);
    private static readonly SKColor ColGateMul    = new(0xFF_FFD60A);
    private static readonly SKColor ColGateGun    = new(0xFF_BF5AF2);
    private static readonly SKColor ColParticle   = new(0xFF_FF9F0A);
    private static readonly SKColor ColFogWhite   = new(0x00_E8F4F8);
    private static readonly SKColor ColFogFull    = new(0xCC_C8D6E5);
    private static readonly SKColor ColPowerUp    = new(0xFF_5AC8FA);
    private static readonly SKColor ColConcrete   = new(0xFF_8E8E93);
    private static readonly SKColor ColGateClosed = new(0xFF_636366);

    // ── Paints (reused to avoid GC pressure) ─────────────────────────────
    private readonly SKPaint _fillPaint  = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _strokePaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
    private readonly SKPaint _textPaint  = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _shadowPaint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
        Color = new SKColor(0, 0, 0, 80)
    };

    // Font cache – reused with different sizes
    private readonly SKFont _font     = new(SKTypeface.Default);
    private readonly SKFont _fontBold = new(SKTypeface.FromFamilyName(null, SKFontStyle.Bold));

    // ── IRenderer ───────────────────────────────────────────────────────────
    public float Width  { get; private set; }
    public float Height { get; private set; }

    public void Render(SKCanvas canvas, GameState state)
    {
        Width  = state.ViewportWidth;
        Height = state.ViewportHeight;

        var camera = new Camera(Width, Height);

        // Apply screen shake
        canvas.Save();
        if (state.ScreenShake.IsActive)
            canvas.Translate(state.ScreenShake.OffsetX, state.ScreenShake.OffsetY);

        DrawBackground(canvas, camera);
        DrawRoad(canvas, camera);
        DrawObstacles(canvas, camera, state);
        DrawGates(canvas, camera, state);
        DrawPowerUps(canvas, camera, state);
        DrawEnemies(canvas, camera, state);
        DrawEnemyBullets(canvas, camera, state);
        DrawPlayerBullets(canvas, camera, state);
        DrawCrowd(canvas, camera, state);
        DrawPlayer(canvas, camera, state);
        DrawParticles(canvas, state);
        DrawFloatingTexts(canvas, state);

        canvas.Restore();  // undo shake

        DrawHud(canvas, state);
        DrawActiveEffectsHud(canvas, state);

        // Overlay screens
        switch (state.Phase)
        {
            case GamePhase.Menu:    DrawMenuOverlay(canvas, state); break;
            case GamePhase.GameOver: DrawGameOverOverlay(canvas, state); break;
            case GamePhase.Victory:  DrawVictoryOverlay(canvas, state); break;
        }
    }

    // ── Background & road ───────────────────────────────────────────────────

    private void DrawBackground(SKCanvas canvas, Camera camera)
    {
        // Sky gradient above horizon
        using var skyShader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(0, camera.HorizonY),
            [ColSky, ColHorizon],
            null,
            SKShaderTileMode.Clamp);

        _fillPaint.Shader = skyShader;
        canvas.DrawRect(0, 0, Width, camera.HorizonY, _fillPaint);
        _fillPaint.Shader = null;
    }

    private void DrawRoad(SKCanvas canvas, Camera camera)
    {
        // Road is a trapezoid: wide at the bottom, narrow at the horizon
        float horizonLeft  = Width * 0.5f - Width * 0.06f;
        float horizonRight = Width * 0.5f + Width * 0.06f;
        float groundLeft   = 0f;
        float groundRight  = Width;

        using var path = new SKPath();
        path.MoveTo(horizonLeft,  camera.HorizonY);
        path.LineTo(horizonRight, camera.HorizonY);
        path.LineTo(groundRight,  camera.GroundY);
        path.LineTo(groundLeft,   camera.GroundY);
        path.Close();

        using var roadShader = SKShader.CreateLinearGradient(
            new SKPoint(Width * 0.5f, camera.HorizonY),
            new SKPoint(Width * 0.5f, camera.GroundY),
            [ColRoadFar, ColRoadNear],
            null,
            SKShaderTileMode.Clamp);

        _fillPaint.Shader = roadShader;
        canvas.DrawPath(path, _fillPaint);
        _fillPaint.Shader = null;

        // Lane markings — dashed centre line
        DrawLaneMarkings(canvas, camera);
    }

    private void DrawLaneMarkings(SKCanvas canvas, Camera camera)
    {
        // Draw dashed lane dividers between the 3 lanes
        _strokePaint.Color       = ColLane;
        _strokePaint.StrokeWidth = 3f;
        _strokePaint.PathEffect  = SKPathEffect.CreateDash([20f, 20f], 0f);

        for (int divider = 1; divider < GameConstants.LaneCount; divider++)
        {
            float worldX = -GameConstants.WorldHalfWidth + divider * GameConstants.LaneWidth;

            const int steps = 12;
            for (int i = 0; i < steps; i++)
            {
                float d0 = GameConstants.MaxDepth * i / steps;
                float d1 = GameConstants.MaxDepth * (i + 1) / steps;

                var (x0, y0, _) = camera.Project(worldX, d0);
                var (x1, y1, _) = camera.Project(worldX, d1);

                canvas.DrawLine(x0, y0, x1, y1, _strokePaint);
            }
        }

        _strokePaint.PathEffect = null;

        // Road shoulder / kerb lines
        _strokePaint.Color       = new SKColor(0xFF_FFFFFF);
        _strokePaint.StrokeWidth = 2f;

        const float shoulderX = GameConstants.WorldHalfWidth * 0.98f;
        DrawWorldLine(canvas, camera, -shoulderX, GameConstants.MaxDepth, -shoulderX, 0f);
        DrawWorldLine(canvas, camera,  shoulderX, GameConstants.MaxDepth,  shoulderX, 0f);
    }

    private void DrawWorldLine(SKCanvas canvas, Camera camera,
                               float wx0, float d0, float wx1, float d1)
    {
        var (x0, y0, _) = camera.Project(wx0, d0);
        var (x1, y1, _) = camera.Project(wx1, d1);
        canvas.DrawLine(x0, y0, x1, y1, _strokePaint);
    }

    // ── Obstacles ───────────────────────────────────────────────────────────

    private void DrawObstacles(SKCanvas canvas, Camera camera, GameState state)
    {
        foreach (var o in state.Obstacles)
        {
            var (sx, sy, scale) = camera.Project(o.WorldX, o.Depth);
            float w = o.Width  * scale;
            float h = o.Height * scale;
            DrawShadow(canvas, sx, sy, w * 0.9f, h * 0.3f);
            _fillPaint.Color = new SKColor(0xFF_8E8E93);
            canvas.DrawRect(sx - w * 0.5f, sy - h, w, h, _fillPaint);
            ApplyFog(canvas, sx - w * 0.5f, sy - h, w, h, Camera.FogAlpha(o.Depth));
        }
    }

    // ── Gates ───────────────────────────────────────────────────────────────

    private void DrawGates(SKCanvas canvas, Camera camera, GameState state)
    {
        foreach (var gate in state.Gates)
        {
            var (sx, sy, scale) = camera.Project(gate.WorldX, gate.Depth);
            float w = GameConstants.GateWidth  * scale;
            float h = GameConstants.GateHeight * scale;

            SKColor col;
            if (!gate.IsOpen)
            {
                // Closed gate: grey with a lock visual
                col = ColGateClosed;
            }
            else
            {
                col = gate.Operation switch
                {
                    GateOperation.Add        => ColGateAdd,
                    GateOperation.Subtract   => ColGateSub,
                    GateOperation.Multiply   => ColGateMul,
                    GateOperation.UpgradeGun => ColGateGun,
                    _                        => SKColors.White
                };
            }

            // Gate arch
            _fillPaint.Color = col.WithAlpha(gate.IsOpen ? (byte)60 : (byte)120);
            canvas.DrawRect(sx - w * 0.5f, sy - h, w, h, _fillPaint);

            _strokePaint.Color       = col;
            _strokePaint.StrokeWidth = 2f * scale;
            canvas.DrawRect(sx - w * 0.5f, sy - h, w, h, _strokePaint);

            // Label
            string label = gate.IsOpen ? gate.Label : (gate.OnShot == GateHitBehavior.IncrementCounter
                ? $"{gate.HitCounter}/{gate.HitsRemaining}"
                : $"[{gate.HitsRemaining}]");
            float fontSize = System.Math.Max(10f, 18f * scale);
            DrawCenteredText(canvas, label, sx, sy - h * 0.5f, col, fontSize, bold: true);

            ApplyFog(canvas, sx - w, sy - h, w * 2, h, Camera.FogAlpha(gate.Depth));
        }
    }

    // ── Power-ups ────────────────────────────────────────────────────────────

    private void DrawPowerUps(SKCanvas canvas, Camera camera, GameState state)
    {
        foreach (var pu in state.PowerUps)
        {
            var (sx, sy, scale) = camera.Project(pu.WorldX, pu.Depth);
            float r = pu.Radius * scale;

            if (pu.IsBlocked)
            {
                // Concrete block
                _fillPaint.Color = ColConcrete;
                canvas.DrawRoundRect(sx - r, sy - r, r * 2f, r * 2f, r * 0.3f, r * 0.3f, _fillPaint);

                _strokePaint.Color       = ColConcrete.WithAlpha(200);
                _strokePaint.StrokeWidth = 2f * scale;
                canvas.DrawRoundRect(sx - r, sy - r, r * 2f, r * 2f, r * 0.3f, r * 0.3f, _strokePaint);

                // Hit counter
                float fontSize = System.Math.Max(8f, 14f * scale);
                string counterText = $"{pu.BlockHitsRemaining}";
                DrawCenteredText(canvas, counterText, sx, sy + fontSize * 0.3f, SKColors.White, fontSize, bold: true);
            }
            else
            {
                // Revealed power-up — glowing orb
                _fillPaint.Color = ColPowerUp.WithAlpha(40);
                canvas.DrawCircle(sx, sy, r * 1.3f, _fillPaint);

                _fillPaint.Color = ColPowerUp;
                canvas.DrawCircle(sx, sy, r * 0.8f, _fillPaint);

                float fontSize = System.Math.Max(8f, 12f * scale);
                DrawCenteredText(canvas, pu.Label, sx, sy + fontSize * 0.3f, SKColors.White, fontSize, bold: true);
            }

            ApplyFog(canvas, sx - r, sy - r, r * 2f, r * 2f, Camera.FogAlpha(pu.Depth));
        }
    }

    // ── Enemies ─────────────────────────────────────────────────────────────

    private void DrawEnemies(SKCanvas canvas, Camera camera, GameState state)
    {
        // Sort far → near so closer enemies draw on top
        var sorted = state.Enemies.OrderByDescending(e => e.Depth);
        foreach (var enemy in sorted)
        {
            var (sx, sy, scale) = camera.Project(enemy.WorldX, enemy.Depth);
            float r = GameConstants.CrowdMemberRadius * scale;

            // Shadow
            DrawShadow(canvas, sx, sy, r * 2f, r * 0.4f);

            // Humanoid silhouette (mirrors the player crowd, red tint)
            _fillPaint.Color = ColEnemy;
            canvas.DrawCircle(sx, sy - r * 1.4f, r * 0.6f, _fillPaint);                                        // head
            canvas.DrawRoundRect(sx - r * 0.5f, sy - r * 1.3f, r, r * 1.2f, r * 0.3f, r * 0.3f, _fillPaint); // torso

            float fog = Camera.FogAlpha(enemy.Depth);
            ApplyFog(canvas, sx - r, sy - r * 2f, r * 2f, r * 2f, fog);
        }
    }

    // ── Bullets ─────────────────────────────────────────────────────────────

    private void DrawPlayerBullets(SKCanvas canvas, Camera camera, GameState state)
    {
        foreach (var b in state.PlayerBullets)
        {
            var (sx, sy, scale) = camera.Project(b.WorldX, b.Depth);
            float r = b.Radius * scale;
            _fillPaint.Color = ColBulletP;
            canvas.DrawOval(sx, sy, r, r * 2.5f, _fillPaint);
        }
    }

    private void DrawEnemyBullets(SKCanvas canvas, Camera camera, GameState state)
    {
        foreach (var b in state.EnemyBullets)
        {
            var (sx, sy, scale) = camera.Project(b.WorldX, b.Depth);
            float r = b.Radius * scale;
            _fillPaint.Color = ColBulletE;
            canvas.DrawOval(sx, sy, r, r * 2.5f, _fillPaint);
        }
    }

    // ── Crowd ────────────────────────────────────────────────────────────────

    private void DrawCrowd(SKCanvas canvas, Camera camera, GameState state)
    {
        var player = state.Player;
        foreach (var (wx, depth) in state.Crowd.GetMemberPositions(player.WorldX, player.Depth))
        {
            if (depth <= 0f) continue;  // behind camera
            var (sx, sy, scale) = camera.Project(wx, depth);
            float r = GameConstants.CrowdMemberRadius * scale;

            DrawShadow(canvas, sx, sy, r * 2f, r * 0.4f);

            // Body (small humanoid silhouette)
            _fillPaint.Color = ColCrowd;
            canvas.DrawCircle(sx, sy - r * 1.4f, r * 0.6f, _fillPaint);          // head
            canvas.DrawRoundRect(sx - r * 0.5f, sy - r * 1.3f, r, r * 1.2f, r * 0.3f, r * 0.3f, _fillPaint); // torso
        }
    }

    // ── Player ───────────────────────────────────────────────────────────────

    private void DrawPlayer(SKCanvas canvas, Camera camera, GameState state)
    {
        var player = state.Player;
        var (sx, sy, scale) = camera.Project(player.WorldX, player.Depth);
        float r = player.Radius * scale;

        DrawShadow(canvas, sx, sy, r * 2.2f, r * 0.55f);

        bool flashing = player.HitFlashTimer > 0f &&
                        (int)(player.HitFlashTimer * 10f) % 2 == 0;
        SKColor col = flashing ? ColPlayerFlash : ColPlayer;

        // Body
        _fillPaint.Color = col;
        canvas.DrawCircle(sx, sy - r, r, _fillPaint);

        // Gun barrel
        _strokePaint.Color       = col;
        _strokePaint.StrokeWidth = 3f * scale;
        canvas.DrawLine(sx, sy - r * 2f, sx, sy - r * 2.8f, _strokePaint);

        // Eyes
        float eyeR = r * 0.2f;
        _fillPaint.Color = SKColors.White;
        canvas.DrawCircle(sx - r * 0.3f, sy - r * 1.3f, eyeR, _fillPaint);
        canvas.DrawCircle(sx + r * 0.3f, sy - r * 1.3f, eyeR, _fillPaint);
    }

    // ── Effects ──────────────────────────────────────────────────────────────

    private void DrawParticles(SKCanvas canvas, GameState state)
    {
        foreach (var p in state.Particles)
        {
            byte alpha = (byte)(p.AlphaFraction * 255f);
            _fillPaint.Color = ColParticle.WithAlpha(alpha);
            canvas.DrawCircle(p.ScreenX, p.ScreenY, p.Radius * p.AlphaFraction, _fillPaint);
        }
    }

    private void DrawFloatingTexts(SKCanvas canvas, GameState state)
    {
        foreach (var ft in state.FloatingTexts)
        {
            byte alpha = (byte)(ft.AlphaFraction * 255f);
            float size = 22f + (1f - ft.AlphaFraction) * 6f;
            DrawCenteredText(canvas, ft.Text, ft.ScreenX, ft.ScreenY,
                             ColGateAdd.WithAlpha(alpha), size, bold: true);
        }
    }

    // ── HUD ──────────────────────────────────────────────────────────────────

    private void DrawHud(SKCanvas canvas, GameState state)
    {
        if (state.Phase == GamePhase.Menu) return;

        // Score
        DrawText(canvas, $"SCORE  {state.Score:N0}", 16f, 28f, SKColors.White, 18f);
        DrawText(canvas, $"WAVE   {state.Wave}",     16f, 52f, SKColors.White, 14f);

        // Crowd count — top right
        DrawText(canvas, $"× {state.Crowd.Count}", Width - 90f, 28f, ColCrowd, 22f, bold: true);
        DrawText(canvas, "SOLDIERS", Width - 90f, 50f, ColCrowd.WithAlpha(180), 12f);

        // Gun level
        if (state.Player.GunLevel > 0)
            DrawText(canvas, $"GUN LV {state.Player.GunLevel + 1}", 16f, Height - 20f, ColGateGun, 14f);
    }

    /// <summary>Draw active power-up effect icons at the bottom of the screen.</summary>
    private void DrawActiveEffectsHud(SKCanvas canvas, GameState state)
    {
        if (state.Phase == GamePhase.Menu || state.ActiveEffects.Count == 0) return;

        float x = 16f;
        float y = Height - 50f;

        foreach (var effect in state.ActiveEffects)
        {
            string label = effect.Type switch
            {
                PowerUpType.SpeedBoost   => "SPD",
                PowerUpType.Shield       => "SHD",
                PowerUpType.RapidFire    => "RPD",
                PowerUpType.BulletPierce => "PRC",
                PowerUpType.SlowEnemies  => "SLW",
                PowerUpType.FreezeEnemies => "FRZ",
                _ => "?"
            };

            // Timer bar background
            float barW = 40f;
            float barH = 6f;
            _fillPaint.Color = new SKColor(0, 0, 0, 100);
            canvas.DrawRoundRect(x, y + 10f, barW, barH, 2f, 2f, _fillPaint);

            // Timer bar fill
            float fill = System.Math.Clamp(effect.RemainingFraction, 0f, 1f);
            _fillPaint.Color = ColPowerUp;
            canvas.DrawRoundRect(x, y + 10f, barW * fill, barH, 2f, 2f, _fillPaint);

            // Label
            DrawText(canvas, label, x, y + 6f, ColPowerUp, 11f, bold: true);
            x += 50f;
        }
    }

    // ── Overlay screens ──────────────────────────────────────────────────────

    private void DrawMenuOverlay(SKCanvas canvas, GameState state)
    {
        DrawDimOverlay(canvas, 0.6f);
        DrawCenteredText(canvas, "CROWD RUNNER", Width * 0.5f, Height * 0.30f, ColPlayer, 40f, bold: true);

        // Read mode name/description from MapRegistry
        var mapDef = MapRegistry.Get(state.Mode);
        string modeName = mapDef.Name.ToUpperInvariant();
        string modeDesc = mapDef.Description;

        // Mode "button"
        float btnY = Height * 0.46f;
        float btnW = Width * 0.6f;
        float btnH = 52f;

        _fillPaint.Color = ColPlayer.WithAlpha(30);
        canvas.DrawRoundRect(Width * 0.5f - btnW * 0.5f, btnY - btnH * 0.5f, btnW, btnH, 10f, 10f, _fillPaint);

        _strokePaint.Color       = ColPlayer.WithAlpha(120);
        _strokePaint.StrokeWidth = 2f;
        canvas.DrawRoundRect(Width * 0.5f - btnW * 0.5f, btnY - btnH * 0.5f, btnW, btnH, 10f, 10f, _strokePaint);

        DrawCenteredText(canvas, modeName, Width * 0.5f, btnY + 6f, ColPlayer, 22f, bold: true);
        DrawCenteredText(canvas, modeDesc, Width * 0.5f, Height * 0.55f, SKColors.White.WithAlpha(160), 13f);

        DrawCenteredText(canvas, "Tap to Start",  Width * 0.5f, Height * 0.64f, SKColors.White, 22f);
        DrawCenteredText(canvas, "Move left/right with mouse, touch or A/D keys",
                         Width * 0.5f, Height * 0.72f, SKColors.White.WithAlpha(160), 13f);
    }

    private void DrawGameOverOverlay(SKCanvas canvas, GameState state)
    {
        DrawDimOverlay(canvas, 0.75f);
        DrawCenteredText(canvas, "GAME OVER",      Width * 0.5f, Height * 0.38f, ColEnemyShadow.WithAlpha(255), 40f, bold: true);
        DrawCenteredText(canvas, $"Score: {state.Score:N0}", Width * 0.5f, Height * 0.50f, SKColors.White, 24f);
        DrawCenteredText(canvas, "Tap to Restart", Width * 0.5f, Height * 0.62f, SKColors.White, 20f);
    }

    private void DrawVictoryOverlay(SKCanvas canvas, GameState state)
    {
        DrawDimOverlay(canvas, 0.65f);
        DrawCenteredText(canvas, "YOU WIN!",        Width * 0.5f, Height * 0.38f, ColGateAdd, 44f, bold: true);
        DrawCenteredText(canvas, $"Score: {state.Score:N0}", Width * 0.5f, Height * 0.50f, SKColors.White, 24f);
        DrawCenteredText(canvas, "Tap to Play Again", Width * 0.5f, Height * 0.62f, SKColors.White, 20f);
    }

    private void DrawDimOverlay(SKCanvas canvas, float alpha)
    {
        _fillPaint.Color = new SKColor(0, 0, 0, (byte)(alpha * 255f));
        canvas.DrawRect(0, 0, Width, Height, _fillPaint);
    }

    // ── Helper drawing utilities ─────────────────────────────────────────────

    private void DrawShadow(SKCanvas canvas, float cx, float groundY, float w, float h)
    {
        _fillPaint.Color = ColEnemyShadow;
        canvas.DrawOval(cx, groundY, w * 0.5f, h * 0.5f, _fillPaint);
    }

    /// <summary>
    /// Overlay a fog rectangle — blends distant objects into the horizon colour.
    /// </summary>
    private void ApplyFog(SKCanvas canvas, float x, float y, float w, float h, float fogAlpha)
    {
        if (fogAlpha <= 0.01f) return;
        _fillPaint.Color = ColFogFull.WithAlpha((byte)(fogAlpha * 200f));
        canvas.DrawRect(x, y, w, h, _fillPaint);
    }

    private void DrawText(SKCanvas canvas, string text, float x, float y,
                          SKColor color, float size, bool bold = false)
    {
        var font = bold ? _fontBold : _font;
        font.Size = size;
        _textPaint.Color = color;
        canvas.DrawText(text, x, y, SKTextAlign.Left, font, _textPaint);
    }

    private void DrawCenteredText(SKCanvas canvas, string text, float cx, float cy,
                                  SKColor color, float size, bool bold = false)
    {
        var font = bold ? _fontBold : _font;
        font.Size = size;
        _textPaint.Color = color;
        canvas.DrawText(text, cx, cy, SKTextAlign.Center, font, _textPaint);
    }

    // ── IDisposable ──────────────────────────────────────────────────────────

    public void Dispose()
    {
        _fillPaint.Dispose();
        _strokePaint.Dispose();
        _textPaint.Dispose();
        _shadowPaint.Dispose();
        _font.Dispose();
        _fontBold.Dispose();
    }
}
