# Mythology Tower Defense — Celestial Gate Road (Prototype)

A minimal, playable 3D isometric tower-defense prototype built in Unity 6.6 (6000.6.7f1) with URP. Built with Unity primitives and generated materials only — no external assets required. See [`Document/MythologyTowerDefense_Master_Plan.md`](Document/MythologyTowerDefense_Master_Plan.md) for the full design spec and [`Document/PROGRESS.md`](Document/PROGRESS.md) for a milestone-by-milestone status log.

## Requirements

- Unity **6000.6.7f1** (Unity 6.6), with the **WebGL** Build Support module installed if you want to build for the browser.
- No other packages beyond what's already in `Packages/manifest.json`.

## Opening the project

1. Open Unity Hub → Add → select this project folder → open with `6000.6.7f1`.
2. Open the scene `Assets/_Game/Scenes/Main.unity`.
3. Press **Play**.

If `Main.unity` is ever missing or you want to regenerate it from scratch, see "Regenerating the scene" below.

## Controls

- **Left-click an empty gold pad** beside the road: build a Celestial Archer tower (costs 30 gold).
- **Left-click an occupied pad**: select that tower — shows its range ring and a stats panel (range / damage / attack interval).
- **Left-click empty ground**: deselect.
- **Start Wave** button: spawns the current wave (10 enemies, 1 per second).
- **Pause / Resume** button: freezes/unfreezes gameplay (`Time.timeScale`).
- **Restart** button: reloads the scene, resetting all state.

## Project structure

```
Assets/_Game/
  Scripts/
    Core/GameManager.cs                 — game state (Playing/Paused/Won/Lost), pause, restart
    Gameplay/Path/PathDefinition.cs     — ordered waypoints + editor gizmo visualization
    Gameplay/Enemies/EnemyController.cs — waypoint movement, health, defeat/arrival events
    Gameplay/Economy/CurrencyManager.cs — gold balance
    Gameplay/Economy/BaseHealth.cs      — base health, depletion event
    Gameplay/Waves/WaveManager.cs       — spawns waves, tracks active enemies, wires rewards/damage
    Gameplay/Towers/TowerController.cs  — target acquisition, cooldown, firing
    Gameplay/Towers/ProjectileController.cs — homing projectile, damage on impact
    Gameplay/Towers/TowerPad.cs         — build placement, cost check, occupancy
    UI/TowerSelectionController.cs      — new-Input-System mouse raycast for pad clicks/selection
    UI/HUDController.cs                 — currency/health/wave text, buttons, end/selection panels
  Editor/
    PrototypeSceneBuilder.cs            — procedurally builds/saves the battlefield scene
    PrototypeSmokeTest.cs               — headless Edit-Mode logic test (see below)
    WebGLBuildScript.cs                 — scripted WebGL player build
  Scenes/Main.unity                     — the saved, playable prototype scene
  Prefabs/, Art/Materials/              — generated prefabs and URP/Lit materials
```

No namespace collisions: everything gameplay-related lives under `MythologyTowerDefense.*`. There is no `GameConfig` ScriptableObject in this pass — tunable stats are plain `[SerializeField]` fields on each component, matching the values in the master plan (§7). This was a deliberate scope decision to avoid over-engineering the first playable slice; introduce ScriptableObjects later if/when more enemy or tower types are added.

## Regenerating the scene

The battlefield is built procedurally so it can be rebuilt deterministically at any time:

- **Tools ▸ Mythology TD ▸ Build Prototype Scene** — builds into the currently open scene (does not save).
- **Tools ▸ Mythology TD ▸ Build Prototype Scene (and Save)** — builds and saves to `Assets/_Game/Scenes/Main.unity`. Safe to run repeatedly; it clears and rebuilds only the `TDRoot` hierarchy it owns.
- **Tools ▸ Mythology TD ▸ Clear Generated Scene** — removes `TDRoot` and the `EventSystem` without rebuilding.

These are also runnable headlessly, e.g.:

```
Unity.exe -batchmode -nographics -projectPath <path> -executeMethod MythologyTowerDefense.EditorTools.PrototypeSceneBuilder.BuildSceneAndSave -quit
```

## Running the automated test

**Tools ▸ Mythology TD ▸ Run Headless Smoke Test** builds the scene and exercises, via direct method calls and `SendMessage` (no Play Mode required):

- Starting currency (60) and base health (10) initialize correctly.
- An enemy defeated grants its reward exactly once and is unregistered.
- An enemy reaching the base damages it exactly once and is unregistered.
- A tower placed on `TowerPad_00` can actually hit an enemy standing on the real path segment that pad guards (this exact check caught a real bug once — see Known issues/history below).
- Tower placement spends currency, marks the pad occupied, and blocks a second tower on the same pad.
- `GameManager` reaches `Lost` when base health hits zero, and `Won` when `WaveManager` reports all waves complete.

Logs `SMOKE_TEST_RESULT: PASS` or `FAIL - <reason>` to the Console, and exits with a non-zero code on failure when run in batch mode. **Not covered**: real-time wave timing (the 1-second spawn cadence and the "wait for all enemies resolved" coroutine), which only runs in Play Mode — verify that manually.

## Building for WebGL

**Tools ▸ Mythology TD ▸ Build WebGL** builds to `Builds/WebGL/` (git-ignored). It sets `PlayerSettings.WebGL.compressionFormat = Disabled` so the output can be served by any plain static file server without needing a custom `Content-Encoding` header — re-enable Brotli/Gzip compression if deploying to a host that sets that header correctly (smaller download).

Headless:

```
Unity.exe -batchmode -nographics -projectPath <path> -buildTarget WebGL -executeMethod MythologyTowerDefense.EditorTools.WebGLBuildScript.Build -quit
```

To test locally (WebGL builds must be served over HTTP, not opened as a `file://` URL):

```
cd Builds/WebGL
python -m http.server 8080
```

then open `http://localhost:8080`.

## Manual test checklist

This mirrors §12 of the master plan. Items marked ✅ were verified end-to-end (Editor Play Mode and/or the WebGL build) during development; see `Document/PROGRESS.md` for the full log.

- [x] Project opens without compile errors or warnings.
- [x] Prototype scene builds/saves/opens reliably.
- [x] Camera shows entrance, path, base, and all 6 tower pads.
- [x] Enemies follow the route in order, reach the base, orient toward travel direction.
- [x] A tower can be bought when affordable; blocked when not; blocked on an occupied pad.
- [x] Towers attack only enemies within range; projectiles handle a defeated/missing target safely.
- [x] Defeated enemies reward currency exactly once.
- [x] One wave spawns exactly 10 enemies, one per second.
- [x] Pause/Resume and Restart work.
- [x] Victory and defeat panels display and lock out further wave starts.
- [x] WebGL build completes and runs in a browser.

## Known limitations / explicitly postponed

Per the master plan's scope boundaries (§15), the following are **intentionally not implemented** in this prototype: the full mythological tower roster, tower upgrades (the selection panel has a visible but non-functional "Upgrade (Soon)" button placeholder), multiple waves/levels, animated character art, save/load, and any networked/online features. Decorative flourishes (lanterns, cloud motifs, a stylized road texture) were also deferred in favor of gameplay correctness, per the stated priority order (working gameplay → correctness → UI → polish → extra content).

**History note:** an earlier pass placed tower pads 5.5 units from the path centerline while tower range was 4.5 — towers built but could never reach the road. The original smoke test didn't catch this because it teleported its test enemy next to the tower instead of using real pad/path geometry. Both the scene data (pad offset → 3.8) and the test (now positions the enemy on the actual guarded path segment) were fixed; see `Document/PROGRESS.md` for the full trail.
