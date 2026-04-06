# GitHub Copilot Instructions

## Project Overview

**Crowd Runner – YouTube Ad Game** is a pseudo-3D top-down shooter runner built with:
- **Blazor WebAssembly** (.NET 10, C# 14) for the browser host
- **SkiaSharp 3.x** for all rendering (2D canvas with perspective projection)
- No game framework — all engine logic is hand-written in C#

## Repository Structure

```
YouTubeAdGame.slnx                     ← solution file
src/
├── YouTubeAdGame.Engine/              ← platform-independent class library
│   ├── Core/                          ← GameState, GameConstants, IRenderer, IInputProvider
│   ├── Math/                          ← Camera (pseudo-3D projection), MathHelper
│   ├── Objects/                       ← Player, Enemy, Boss, Bullet, Gate, Obstacle, Crowd
│   ├── Effects/                       ← ScreenShake, FloatingText, Particle
│   ├── Engine/                        ← GameEngine (update loop), SpawnSystem
│   └── Rendering/                     ← SkiaGameRenderer
└── YouTubeAdGame.Web/                 ← Blazor WebAssembly host
    ├── Components/GameCanvas.razor    ← SKCanvasView + 60 Hz PeriodicTimer game loop
    ├── Input/BlazorInputProvider.cs   ← mouse/touch/keyboard → IInputProvider
    └── Pages/Home.razor               ← full-screen game page
.github/workflows/deploy.yml          ← CI build + GitHub Pages deploy
```

## Architecture

All game logic lives in **YouTubeAdGame.Engine** — no Blazor, no platform dependencies.
The Web project is a thin host that wires up input and rendering.

```
IInputProvider  →  GameEngine.Update(state, dt)
                       ↓
                   GameState  (single source of truth for all mutable data)
                       ↓
IRenderer.Render(canvas, state)  →  SkiaGameRenderer
```

- **GameState** is the only mutable shared object; it is passed by reference into every update and render call.
- **GameEngine.Update(GameState, float dt)** runs at 60 Hz (fixed timestep via `PeriodicTimer` in `GameCanvas.razor`).
- **SpawnSystem** controls enemy waves and gate spawning.
- **SkiaGameRenderer** performs all drawing using SkiaSharp `SKCanvas`.

### Pseudo-3D Camera

Objects carry a **depth** value (0 = at the player, 1000 = horizon). `Camera.cs` projects world → screen:

- `screenY` = lerp(playerY, horizonY, t)  where `t = depth / MaxDepth`
- `scale`   = lerp(NearScale, FarScale, t)
- `screenX` = centreX + worldX × (screenHalfWidth / worldHalfWidth)

## Coding Conventions

- **C# 14 / .NET 10** — use the latest language features (primary constructors, collection expressions `[]`, raw string literals, etc.).
- **Nullable reference types enabled** — always annotate nullability correctly; avoid `!` suppression unless unavoidable.
- **Implicit usings enabled** — do not add redundant `using` directives for common namespaces.
- **File-scoped namespaces** — every `.cs` file uses `namespace Foo.Bar;` (no braces).
- **XML doc comments** on all `public` members — use `/// <summary>…</summary>` style.
- **Constants in `GameConstants.cs`** — all numeric tuning values (speeds, radii, timers, etc.) must be defined as `public const float` in `GameConstants`. Never use magic numbers inline.
- **No platform code in Engine** — `YouTubeAdGame.Engine` must not reference Blazor, ASP.NET, or any browser API. Keep it portable (MAUI, WPF, desktop compatible).
- **SkiaSharp paint reuse** — create `SKPaint` objects once and reuse them; avoid allocating new paints inside the render loop.
- **Game objects inherit `GameObjectBase`** — new world objects should extend `GameObjectBase` and be stored in the appropriate `List<T>` on `GameState`.

## Key Files

| File | Purpose |
|------|---------|
| `Core/GameConstants.cs` | All tuning constants |
| `Core/GameState.cs` | All mutable runtime state |
| `Core/IInputProvider.cs` | Input abstraction + `InputState` |
| `Core/IRenderer.cs` | Rendering abstraction |
| `Math/Camera.cs` | Pseudo-3D projection math |
| `Engine/GameEngine.cs` | Fixed-timestep update, collision, end-conditions |
| `Engine/SpawnSystem.cs` | Wave and gate spawning |
| `Rendering/SkiaGameRenderer.cs` | Full scene rendering |
| `Components/GameCanvas.razor` | 60 Hz loop, SKCanvasView wiring |
| `Input/BlazorInputProvider.cs` | Browser input → `IInputProvider` |

## Build & Run

```bash
# Requires .NET 10 SDK
dotnet workload install wasm-tools   # SkiaSharp native WASM assets
dotnet build YouTubeAdGame.slnx
dotnet run --project src/YouTubeAdGame.Web
```

## Extending the Engine

| Feature | Where to add |
|---------|-------------|
| New weapon | `Engine/GameEngine.cs` + `Rendering/SkiaGameRenderer.cs` |
| New enemy behaviour | `Objects/Enemy.cs` + `Engine/SpawnSystem.cs` |
| New gate operation | `Objects/Gate.cs` `GateOperation` enum |
| New visual effect | `Effects/` + `Rendering/SkiaGameRenderer.cs` |
| Sound | New `IAudioProvider` abstraction in `Core/`, implement per platform |
| MAUI / desktop port | Implement `IRenderer` + `IInputProvider` in a new project |
