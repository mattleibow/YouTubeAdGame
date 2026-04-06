# GitHub Copilot Instructions

> **⚠️ Keep these instructions up to date.**
> Every time you add a feature, change behaviour, or update architecture, edit this file as part of the same PR.
> This file is the single source of truth for all agents working on this repository.
> See [Keeping These Instructions Up to Date](#keeping-these-instructions-up-to-date) at the bottom for the rule.

## Project Overview

**Crowd Runner – YouTube Ad Game** is a pseudo-3D top-down shooter runner built with:
- **Blazor WebAssembly** (.NET 10, C# 14) for the browser host
- **SkiaSharp 3.x** for all rendering (2D canvas with perspective projection)
- No game framework — all engine logic is hand-written in C#

### Core Concept — Army vs Zombies

The player commands a **crowd of soldiers** marching down the road toward an endless **horde of zombies**.

- **Soldiers** (the player's crowd): automatically fire bullets — the more soldiers, the more bullets and the wider the spread. Gun-level gates multiply bullet density further. Soldiers are the "lives" — lose them all and it's game over.
- **Zombies** (enemies): shuffle slowly toward the player. They **never fire**. When a zombie reaches the player they consume one soldier and die.
- Gates on the road offer math operations (`+`, `-`, `×`) to gain/lose soldiers, or gun upgrades.

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
    ├── Components/GameCanvas.razor    ← SKCanvasView + 60 Hz game loop + debug inspector
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
- **SpawnSystem** controls enemy streaming and gate spawning.
- **SkiaGameRenderer** performs all drawing using SkiaSharp `SKCanvas`.

### Pseudo-3D Camera

Objects carry a **depth** value (0 = at the player, 1000 = horizon). `Camera.cs` projects world → screen:

- `screenY` = lerp(playerY, horizonY, t)  where `t = depth / MaxDepth`
- `scale`   = lerp(NearScale, FarScale, t)
- `screenX` = centreX + worldX × (screenHalfWidth / worldHalfWidth)

### Zombie Spawning — Sea-of-Zombies Model

Zombies spawn in **batches** (`GameConstants.ZombiesPerSpawn = 8`) every `state.SpawnInterval` seconds (default 100 ms) from the far horizon (`SpawnDepth`). Each zombie in the batch gets a slight random depth jitter so they don't all appear on the same line. The field builds up rapidly into a sea effect.

- `state.MaxEnemiesOnScreen` (default 500, max slider 1–1000) caps the total zombie count.
- `state.SpawnInterval` (default 0.1 s, slider range 50–2000 ms) is runtime-adjustable via the inspector.
- Wave progression reduces the effective interval slightly: `max(0.05s, SpawnInterval - Wave × 0.002s)`.

### Crowd-Based Firing

The player's soldiers fire automatically every `PlayerFireRate` seconds. Bullet count scales with crowd size:

```
count = min(Crowd.Count / 5 × (1 + GunLevel × 0.5), 50)
```

Bullets are spread uniformly across `±CrowdHalfWidth` world-units centred on the player. Gun-level gates increase bullet density. There are **no enemy bullets** — zombies only move.

### Player / Crowd Boundary

The player's horizontal position is clamped to `±(WorldHalfWidth − CrowdHalfWidth)` so the entire crowd formation stays within road edges at all times.

### Debug Inspector Panel

`GameCanvas.razor` renders a dark sidebar beside the game canvas that shows live:

| Stat | Description |
|------|-------------|
| Phase / Wave / Score / Distance | Overall game progression |
| Soldiers | `state.Crowd.Count` |
| Zombies | `state.Enemies.Count` |
| Bullets | `state.PlayerBullets.Count` |
| Particles | `state.Particles.Count` |
| Max zombies slider | Mutates `state.MaxEnemiesOnScreen` in real time (1–1000) |
| Spawn delay slider | Mutates `state.SpawnInterval` in real time (50–2000 ms) |

Inspector labels use plain ASCII — no emoji (default SkiaSharp font does not support emoji).

Stats refresh every 10 game frames (~6 Hz) via `InvokeAsync(StateHasChanged)`.  
Layout: `.page-layout` flex row → `.game-container` (flex: 1) + `.inspector-panel` (200 px fixed width).

## CI / Deployment

`.github/workflows/deploy.yml` builds and publishes the Blazor WASM app to **GitHub Pages**.

The **deploy job** runs when:
- Pushing to `main` or `master`, **or**
- A **pull request** is open and has the `deploy` label.

This means you can preview any PR branch on GitHub Pages simply by adding the `deploy` label — no merge required.

## Coding Conventions

- **C# 14 / .NET 10** — use the latest language features (primary constructors, collection expressions `[]`, raw string literals, etc.).
- **Nullable reference types enabled** — always annotate nullability correctly; avoid `!` suppression unless unavoidable.
- **Implicit usings enabled** — do not add redundant `using` directives for common namespaces.
- **File-scoped namespaces** — every `.cs` file uses `namespace Foo.Bar;` (no braces).
- **XML doc comments** on all `public` members — use `/// <summary>…</summary>` style.
- **Constants in `GameConstants.cs`** — all numeric tuning values (speeds, radii, timers, etc.) must be defined as `public const float` in `GameConstants`. Never use magic numbers inline.
- **Runtime tuning via `GameState`** — values that the player/developer needs to adjust at runtime (e.g. `MaxEnemiesOnScreen`) live as mutable properties on `GameState`, defaulting to a `GameConstants` value. Do not hard-code them at the call site.
- **No platform code in Engine** — `YouTubeAdGame.Engine` must not reference Blazor, ASP.NET, or any browser API. Keep it portable (MAUI, WPF, desktop compatible).
- **SkiaSharp paint reuse** — create `SKPaint` objects once and reuse them; avoid allocating new paints inside the render loop.
- **Game objects inherit `GameObjectBase`** — new world objects should extend `GameObjectBase` and be stored in the appropriate `List<T>` on `GameState`.

## Key Files

| File | Purpose |
|------|---------|
| `Core/GameConstants.cs` | All tuning constants (default values) |
| `Core/GameState.cs` | All mutable runtime state, including runtime-adjustable tuning properties |
| `Core/IInputProvider.cs` | Input abstraction + `InputState` |
| `Core/IRenderer.cs` | Rendering abstraction |
| `Math/Camera.cs` | Pseudo-3D projection math |
| `Engine/GameEngine.cs` | Fixed-timestep update, collision, end-conditions |
| `Engine/SpawnSystem.cs` | Zombie batch spawning and gate spawning |
| `Rendering/SkiaGameRenderer.cs` | Full scene rendering |
| `Components/GameCanvas.razor` | 60 Hz loop, SKCanvasView wiring, debug inspector panel |
| `Input/BlazorInputProvider.cs` | Browser input → `IInputProvider` |
| `wwwroot/css/app.css` | Global styles including `.page-layout` and `.inspector-panel` |
| `.github/workflows/deploy.yml` | CI build + conditional GitHub Pages deploy |

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
| New debug stat | Add to inspector HTML in `GameCanvas.razor` |
| New runtime tuning knob | Add mutable property to `GameState` (default from `GameConstants`); wire up a slider/toggle in the inspector (use ASCII labels — no emoji) |
| Sound | New `IAudioProvider` abstraction in `Core/`, implement per platform |
| MAUI / desktop port | Implement `IRenderer` + `IInputProvider` in a new project |

## Keeping These Instructions Up to Date

**This file must be updated in every PR that changes features, architecture, or developer workflow.**

Specifically, whenever you:
- Add or remove a game feature → update the relevant section and the Key Files table.
- Change how spawning, collision, or any engine system works → update the Architecture section.
- Add a new tuning knob or debug control → document it under Debug Inspector Panel.
- Change the CI/deploy workflow → update the CI / Deployment section.
- Add a new key file or rename one → update the Key Files table.

Treat this file the way you treat `GameConstants.cs`: it is the authoritative reference, and leaving it stale is a bug.
