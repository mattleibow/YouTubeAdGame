# Crowd Runner – YouTube Ad Game

A pseudo-3D top-down shooter runner built with Blazor WebAssembly, .NET 10, C# 14, and SkiaSharp.
The player stands at the bottom of the screen, enemies scroll toward them from the top, and the crowd grows or shrinks as you pass through gates.

## Play Online

After the GitHub Actions workflow completes, the game is available at:
`https://<your-username>.github.io/YouTubeAdGame/`

## Controls

| Input | Action |
|-------|--------|
| Mouse drag / touch | Move player left/right |
| A / ← | Move left |
| D / → | Move right |
| Space / Enter | Start / restart |

Shooting is automatic.

## Project Structure

```
YouTubeAdGame.slnx                     ← solution
src/
├── YouTubeAdGame.Engine/              ← shared class library (platform-independent)
│   ├── Core/
│   │   ├── GameConstants.cs           ← tuning values & constants
│   │   ├── GameState.cs               ← all runtime game state
│   │   ├── IRenderer.cs               ← rendering abstraction
│   │   └── IInputProvider.cs          ← input abstraction
│   ├── Math/
│   │   └── Camera.cs                  ← pseudo-3D projection + MathHelper
│   ├── Objects/
│   │   ├── GameObjectBase.cs          ← base world object
│   │   ├── Player.cs
│   │   ├── Enemy.cs + Boss.cs
│   │   ├── Bullet.cs
│   │   ├── Gate.cs                    ← Add / Subtract / Multiply / UpgradeGun
│   │   ├── Obstacle.cs
│   │   └── Crowd.cs                   ← count + grid member positions
│   ├── Effects/
│   │   ├── ScreenShake.cs             ← trauma-based shake
│   │   ├── FloatingText.cs            ← +20, ×3, etc.
│   │   └── Particle.cs                ← death burst particles
│   ├── Engine/
│   │   ├── GameEngine.cs              ← fixed-timestep update, collisions
│   │   └── SpawnSystem.cs             ← waves, gates
│   └── Rendering/
│       └── SkiaGameRenderer.cs        ← full pseudo-3D SkiaSharp scene
│
└── YouTubeAdGame.Web/                 ← Blazor WebAssembly host
    ├── Components/
    │   └── GameCanvas.razor           ← SKCanvasView + 60 Hz game loop
    ├── Input/
    │   └── BlazorInputProvider.cs     ← mouse / touch / keyboard → IInputProvider
    ├── Pages/Home.razor               ← full-screen game page
    └── wwwroot/index.html             ← PWA shell

.github/workflows/deploy.yml          ← CI build + GitHub Pages deploy
```

## Architecture

All game logic, math, and rendering live in **YouTubeAdGame.Engine**, with no dependency on Blazor.
The engine can be reused in MAUI, WPF, or any other platform that supports SkiaSharp.

```
IInputProvider  →  GameEngine.Update(state, dt)
                       ↓
                   GameState (all mutable data)
                       ↓
IRenderer.Render(canvas, state)  →  SkiaGameRenderer
```

### Pseudo-3D Camera

Objects have a **depth** (0 = at player, 1000 = horizon). The camera projects world coordinates to screen space:

- `screenY` = lerp(playerY, horizonY, t)
- `scale` = lerp(1.0, 0.08, t)
- `screenX` = centreX + worldX × (screenHalfWidth / worldHalfWidth)

This gives the perspective illusion without a true 3D engine.

## Building Locally

```bash
# requires .NET 10 SDK
dotnet workload install wasm-tools   # for SkiaSharp native WASM support
dotnet build YouTubeAdGame.slnx
dotnet run --project src/YouTubeAdGame.Web
```

## CI / CD

GitHub Actions (`.github/workflows/deploy.yml`):

1. **Build job** – restores, builds, and publishes the Blazor WASM app on every push and pull request.
2. **Deploy job** – uploads the `wwwroot` folder as a GitHub Pages artifact (runs only on `main` / `master`).

Enable GitHub Pages in your repository settings: **Settings → Pages → Source: GitHub Actions**.

## Extending the Engine

| Feature | Where to add |
|---------|-------------|
| New weapon type | `GameEngine.cs` + `SkiaGameRenderer.cs` |
| New enemy behaviour | `Objects/Enemy.cs` + `Engine/SpawnSystem.cs` |
| Boss fight | `Objects/Enemy.cs` Boss class (stub included) |
| New gate operation | `Objects/Gate.cs` GateOperation enum |
| Level progression | `Engine/SpawnSystem.cs` + `GameConstants.cs` |
| Sound effects | New `IAudioProvider` abstraction in `Core/` |
| MAUI port | Implement `IRenderer` + `IInputProvider` in new project |
