# MYTHOLOGY TOWER DEFENSE --- MASTER PLAN

**Project:** MythologyTowerDefense\
**Engine:** Unity 6.6 (6000.6.7f1)\
**Render pipeline:** Universal Render Pipeline (URP)\
**Target:** PC development first, then WebGL/browser\
**Document purpose:** Single source of truth for Claude Code. Read this
file before implementing or changing the game.

------------------------------------------------------------------------

## 1. Instructions for Claude Code

You are the lead Unity gameplay engineer, technical designer, and
implementation agent for this project.

### Operating rules

1.  Read this entire document before making changes.
2.  Inspect the actual Unity project, packages, scenes, input settings,
    and existing scripts. Do not assume file paths or package versions.
3.  Preserve existing project files unless there is a clear reason to
    change them. Never delete or overwrite user work without explaining
    why first.
4.  Work in small, testable milestones. After each milestone:
    -   list files created/changed;
    -   explain how to test the milestone;
    -   run available checks/builds where possible;
    -   report errors honestly.
5.  Do not claim something compiles, runs, or builds unless you actually
    verified it. If Unity Editor automation is unavailable, clearly
    state what was not tested and give exact manual test steps.
6.  Prefer simple, maintainable C# and Unity APIs supported by Unity
    6.6. Avoid unnecessary packages and deprecated APIs.
7.  Use Unity primitives, procedural meshes, simple materials, and
    generated UI for the first playable prototype. Do not block progress
    on paid assets or external downloads.
8.  Never put secrets, API keys, passwords, or personal tokens in the
    repository.
9.  Use Git safely. Inspect `git status` before making changes. Do not
    run destructive commands such as `git reset --hard`,
    `git clean -fd`, or mass deletion unless the user explicitly
    approves.
10. If a decision is missing, choose a reasonable low-risk default,
    document it, and keep going. Ask the user only when a decision is
    genuinely blocking.
11. Do not spend time asking the user to manually create files that you
    can safely create yourself.
12. The project must remain playable in the Unity Editor and be prepared
    for WebGL. Do not switch to a different engine.

### First response

Before implementation, briefly report: - what you found in the current
project; - any blockers; - a milestone plan; - whether Git is
initialized; - whether the installed project currently has WebGL build
support available.

Then begin Milestone 1. Do not stop after producing only a plan.

------------------------------------------------------------------------

## 2. Game vision

Create a polished, approachable, 3D isometric tower-defense game
inspired by Chinese mythology. The player protects a sacred realm from
waves of demons and corrupted spirits by placing mythological guardian
towers beside a winding road.

### Core experience

-   Easy to understand: enemies travel along a visible winding route
    toward a base.
-   Strategic: towers have distinct ranges, damage, attack speeds, and
    special effects.
-   Charming: stylized chibi/fantasy visuals, expressive silhouettes,
    readable colors.
-   Mythological: original interpretations of figures and motifs such as
    Pangu (Bàn Cổ), Nuwa (Nữ Oa), and Sun Wukong (Tôn Ngộ Không), plus
    original demon designs.
-   Browser-friendly: simple materials, controlled polygon count,
    scalable effects, short loading time.
-   Expandable: build a reliable vertical slice first; add content only
    after the loop is stable.

Do not copy copyrighted character designs, models, UI, names, music, or
art from existing games. Use original stylized designs and treat
mythology as inspiration.

------------------------------------------------------------------------

## 3. Target platforms and technical constraints

### Primary development

-   Windows PC
-   Unity 6.6 / URP
-   C#
-   Git for version control
-   VS Code + Claude Code for code authoring

### Deployment target

-   WebGL / browser build after the core game works in Editor.
-   Design UI for mouse input and common desktop browser resolutions.
-   Avoid platform-specific APIs that do not work in WebGL.
-   Do not assume multithreading, filesystem access, or networking
    features behave like desktop builds.
-   Keep memory and draw calls modest; avoid large textures and
    expensive post-processing.

### Project conventions

-   Namespace: `MythologyTowerDefense` (or a clearly consistent
    sub-namespace).
-   Use serialized fields for tunable values.
-   Avoid giant monolithic scripts.
-   Use ScriptableObjects for reusable tower/enemy/wave configuration
    when useful, but do not overengineer the first prototype.
-   Keep gameplay logic separate from UI and visual effects.
-   Use clear naming, null checks, and concise comments for non-obvious
    logic.

### Suggested folder layout

Adapt to the actual project rather than duplicating existing folders.

``` text
Assets/
  _Game/
    Art/
      Materials/
      Models/
      VFX/
    Audio/
    Prefabs/
      Enemies/
      Towers/
      Projectiles/
      Environment/
      UI/
    Scenes/
    Scripts/
      Core/
      Gameplay/
        Enemies/
        Towers/
        Waves/
        Economy/
        Path/
      UI/
      Utilities/
    ScriptableObjects/
      Enemies/
      Towers/
      Waves/
    Settings/
    Tests/
    Editor/
```

------------------------------------------------------------------------

## 4. Visual direction and art bible

### Style

-   Stylized 3D, cute chibi proportions, fantasy strategy-game
    presentation.
-   Isometric / three-quarter top-down camera.
-   Rounded shapes, clear silhouettes, readable animations.
-   Rich but controlled palette: jade green, warm gold, cinnabar red,
    deep teal, stone gray, cream highlights.
-   Soft ambient lighting and restrained shadows.
-   Keep the battlefield and enemy path readable above decorative
    detail.

### Environment

-   Sacred mountain / celestial garden / ancient Chinese-inspired
    fantasy courtyard.
-   Stone or pale jade road winding through grass, rock, lanterns, and
    stylized trees.
-   Decorative gates, banners, cloud motifs, and small shrines may frame
    the battlefield.
-   Keep scenery outside the walkable route and tower pads.
-   Use simple primitive geometry and procedural placement initially;
    replace with custom assets later.

### Camera

-   Fixed isometric-style camera at a three-quarter angle.
-   Orthographic camera is preferred for predictable strategy-game
    readability; perspective is acceptable only if it materially
    improves the look without hurting placement clarity.
-   Center the map and fit it to common 16:9 screens.
-   Provide mouse-wheel zoom only if stable and useful; avoid complex
    camera controls in the first milestone.
-   Ensure enemies, towers, UI, and path are visible without excessive
    panning.

### Readability rules

-   Enemy route must have a strong contrasting surface.
-   Enemies must remain visible against the ground.
-   Tower pads must be visually distinct and clearly separated from the
    road.
-   Targeting range should be visible when a tower is selected or
    placed.
-   Effects must not obscure enemies or UI.
-   Do not put trees, props, tower pads, colliders, or towers on the
    enemy route.

### Initial asset policy

For the first playable version, use: - Unity primitives and procedural
meshes; - simple URP-compatible materials; - basic particle effects or
small procedural effects; - placeholder shapes for characters; -
built-in UI or Unity UI. Do not require Blender or external models
before the game loop is playable.

------------------------------------------------------------------------

## 5. Game loop

1.  The player enters a level and sees the winding path, base, enemy
    entrance, and tower pads.
2.  The player starts a wave.
3.  Enemies spawn at the entrance and follow the path to the base.
4.  Towers automatically detect and attack enemies within range.
5.  Defeated enemies grant currency.
6.  Enemies reaching the base reduce base health.
7.  Between or during waves, the player spends currency to place or
    upgrade towers.
8.  The player wins by surviving all configured waves; the player loses
    if base health reaches zero.
9.  The player can pause/resume or restart.

The initial prototype only needs one level, one enemy, one tower, and
one wave of ten enemies. Build architecture so more content can be added
later without rewriting the whole game.

------------------------------------------------------------------------

## 6. Map and route design

### Layout

-   One compact battlefield designed for a 16:9 view.
-   A winding route with several clear turns, not a straight line.
-   Entrance at one side of the map; protected base/portal at the
    opposite side.
-   Route length must be sufficient for towers to engage enemies for
    multiple seconds.
-   Add at least 3 visible, legal tower pads beside the road for the
    prototype; later expand to 6--10 pads if screen space permits.

### Path implementation

-   Store route as an ordered list of world-space waypoints.
-   Enemy moves from waypoint to waypoint at configurable speed.
-   Use a small arrival threshold and advance waypoint index safely.
-   Enemy movement must be frame-rate independent.
-   Do not let enemies cut corners in a way that makes the route
    unclear. Smooth turns can be added after the basic route works.
-   Route visualization must match the actual waypoint path.
-   Add gizmos in Editor for waypoint debugging.
-   Enemies should orient toward their direction of movement if a visual
    model exists.

### Tower placement

-   Placement pads are separate from the road and visually readable.
-   Each pad may contain a placement marker and an optional tower
    socket.
-   For the first version, towers can be placed only on predefined pads;
    free placement is not required.
-   Clicking an empty pad opens a small tower choice or places the only
    available archer tower if affordable.
-   Clicking an occupied pad selects the tower and displays its range
    and upgrade information.
-   The route is never a valid placement surface.
-   Prevent duplicate tower placement on the same pad.

------------------------------------------------------------------------

## 7. Initial content design

### 7.1 First level: "Đường Thiên Môn" / "Celestial Gate Road"

Theme: a sacred mountain pass leading to a celestial gate.

Visual landmarks: - Jade-stone road with gold accents. - Grass and stone
terrain. - Small lanterns and stylized cloud decorations. - Enemy
entrance marked by a dark portal or demonic gate. - Base represented by
a glowing celestial gate or protective crystal.

Keep decoration lightweight and out of the path and pad areas.

### 7.2 First enemy: "Tiểu Yêu" / "Lesser Yaoguai"

Role: basic, balanced enemy. - Appearance: small chibi demon, dark
teal/purple body, glowing eyes, simple horns. - Movement: follows the
waypoints toward the base. - Stats starting point: - Health: 40 - Move
speed: 2.0 world units/second - Base damage on arrival: 1 - Reward on
defeat: 8 currency - Use placeholder geometry first; a model can be
added later. - Health bar above the enemy is optional for the first
milestone but desirable if simple.

### 7.3 First tower: "Thiên Cung Xạ Thủ" / "Celestial Archer"

Role: single-target ranged damage. - Appearance: stylized guardian
statue or small archer platform in jade, cream, and gold. - Attack:
automatically selects an enemy in range and fires a visible
projectile. - Starting stats: - Cost: 30 currency - Range: 4.5 world
units - Damage: 12 - Attack interval: 0.8 seconds - Projectile speed: 8
world units/second - Target priority: first enemy along the route (the
enemy closest to the base), unless that is complex; document and keep it
deterministic. - Projectile should move toward the target and deal
damage on impact. It should handle a target being destroyed or
disappearing without errors. - Range indicator appears on selection. -
Avoid firing every frame; use a cooldown/attack timer.

### 7.4 Economy

-   Starting currency: 60
-   Tower cost: 30
-   Enemy reward: 8
-   Currency cannot go below zero.
-   Buying a tower checks affordability before placement.
-   Show current currency in the UI.
-   Avoid double rewards if an enemy is destroyed by multiple effects.

### 7.5 Base

-   Starting base health: 10
-   Each enemy reaching the base removes 1 health.
-   When health reaches zero, trigger game-over state and stop spawning
    new enemies.
-   Show base health in the UI.

### 7.6 Wave

Initial prototype: - 10 enemies. - Spawn interval: 1.0 second. - Spawn
one enemy at a time. - Do not mark a wave complete until all enemies
have spawned and all spawned enemies have either been defeated or
reached the base. - Show wave state and remaining enemies. - Allow
starting the first wave with a button. - Do not permit duplicate
overlapping waves in the initial version.

### 7.7 Later mythological tower roster (not required for the first playable build)

Use original, stylized interpretations and balance them around distinct
tactical roles.

  -----------------------------------------------------------------------
  Character / tower       Fantasy role            Possible gameplay
                                                  identity
  ----------------------- ----------------------- -----------------------
  Nữ Oa / Nuwa            Support / healing       Repairs base or buffs
                                                  nearby towers

  Bàn Cổ / Pangu          Heavy area damage       Slow, powerful strikes
                                                  with splash damage

  Tôn Ngộ Không / Sun     Fast attacker           Rapid attacks, chain
  Wukong                                          hits, or a clone effect

  Hậu Nghệ / Hou Yi       Long-range precision    Long range and high
                                                  single-target damage

  Lôi Công-inspired       Lightning control       Chains lightning
  guardian                                        between enemies

  Original jade guardian  Slow / control          Slows enemies with jade
                                                  magic
  -----------------------------------------------------------------------

These are future ideas only. Do not implement the full roster before the
basic loop is stable.

------------------------------------------------------------------------

## 8. Controls and user interface

### Desktop controls

-   Left-click an empty tower pad: select/build a tower.
-   Left-click an occupied tower pad: select that tower.
-   Click outside a selection: deselect if practical.
-   Buttons: Start Wave, Pause/Resume, Restart.
-   Mouse wheel zoom is optional and may be postponed.

### HUD

A clean, compact fantasy UI with high contrast and minimal clutter.

Required: - Currency display with a coin icon or simple symbol. - Base
health display. - Wave number/status. - Start Wave button. -
Pause/Resume button. - Restart button. - Game-over / victory panel. -
Optional selected-tower panel with range, damage, attack interval, and
upgrade button placeholder.

### UI style

-   Dark teal / deep jade panels.
-   Gold or cream text accents.
-   Cinnabar red for danger or game over.
-   Large readable text suitable for browser windows.
-   Use Unity UI or another existing project UI approach; do not add a
    dependency solely for decoration.
-   Ensure layout scales to different aspect ratios.
-   Do not rely on emoji rendering for essential icons; use text or
    simple shapes if needed.

------------------------------------------------------------------------

## 9. Architecture proposal

Adapt this architecture to existing project conventions. Do not force
abstractions that make the prototype harder to maintain.

### Core systems

-   `GameManager`: game state, start/pause/restart, win/lose state.
-   `LevelBootstrapper` or Editor setup tool: constructs initial scene
    if needed.
-   `PathDefinition`: ordered waypoints and debug visualization.
-   `EnemyController`: movement, health, base arrival.
-   `EnemySpawner` / `WaveManager`: wave lifecycle and spawn timing.
-   `TowerPad`: handles click, occupancy, and placement.
-   `TowerController`: target detection, cooldown, attack.
-   `ProjectileController`: movement, impact, damage.
-   `CurrencyManager`: spending and rewards.
-   `BaseHealth`: base health and game-over notification.
-   `HUDController`: updates currency, health, wave state, buttons, and
    panels.
-   `GameConfig` / ScriptableObjects: tunable stats where useful.

### Event flow

-   Enemy defeated → reward currency exactly once → remove enemy.
-   Enemy reaches base → reduce base health exactly once → remove enemy.
-   Base health reaches zero → GameManager enters Lost state → stop wave
    spawning and gameplay.
-   All wave enemies resolved → wave complete → enable next-wave action
    or show victory if no more waves configured.
-   Tower placed → deduct currency only after validation succeeds → mark
    pad occupied.
-   Pause → stop gameplay time and prevent gameplay actions where
    appropriate.
-   Restart → reset enemies, towers, currency, base health, wave state,
    and UI consistently.

### Engineering rules

-   Avoid hard-coded scene object searches every frame.
-   Avoid hidden dependencies on object names where serialized
    references are possible.
-   Avoid static global state unless there is a clear reason.
-   Prevent double death, double rewards, double base damage, and
    duplicate wave starts.
-   Clean up spawned enemies/projectiles on restart.
-   Handle missing references with useful warnings instead of silent
    failures.
-   Keep scripts focused and reasonably small.
-   Use `Time.deltaTime` or equivalent appropriate timing, and ensure
    pause behavior is consistent.

------------------------------------------------------------------------

## 10. Scene setup and automation

Claude should choose the safest implementation path after inspecting the
project.

Preferred options: 1. Create a runtime bootstrapper that constructs the
prototype scene when entering Play mode; or 2. Create an Editor setup
menu/tool that builds the scene and saves it to
`Assets/_Game/Scenes/Main.unity`.

Prefer a saved scene for reliable future development, but runtime
bootstrap is acceptable for a very first proof of concept if it is
clearly documented.

If creating an Editor setup tool: - Put it under an `Editor` folder. -
Expose a clearly named menu item such as
`Tools/Mythology TD/Build Prototype Scene`. - Do not run
scene-generation destructively without confirming which scene will be
replaced. - Ensure the setup tool is safe to run more than once,
avoiding duplicate objects. - Save generated scene/assets inside
`Assets/_Game/`. - Include instructions for running it. - Make the
initial scene openable and playable by pressing Play.

If automation requires Unity Editor to be open or requires manual
compilation, explain that step precisely.

------------------------------------------------------------------------

## 11. Milestones and acceptance criteria

### Milestone 0 --- Project audit and safety

Tasks: - Inspect folder structure, scene, packages, input configuration,
and Git state. - Confirm Unity version and available WebGL support from
project/environment where possible. - Identify missing prerequisites
without changing unrelated settings. Acceptance: - A concise audit
report and implementation plan. - No project files deleted.

### Milestone 1 --- Greybox battlefield

Tasks: - Create ground, winding road, entrance, base, camera, lighting,
and at least 3 tower pads. - Make road and pads visibly distinct. - Save
a playable scene or provide a deterministic setup tool. Acceptance: -
Scene opens without missing references. - Camera shows the whole route
and all pads. - No pad overlaps the road.

### Milestone 2 --- Enemy and base

Tasks: - Create the Lesser Yaoguai placeholder. - Follow waypoints
correctly. - Damage base on arrival. - Base health UI updates.
Acceptance: - Enemy traverses the entire route without teleporting or
getting stuck. - Each enemy damages the base once.

### Milestone 3 --- Archer tower and combat

Tasks: - Build tower on an empty pad after validating currency. -
Acquire targets and fire projectiles. - Apply damage and remove defeated
enemies. - Award currency once per defeated enemy. Acceptance: - Tower
only attacks within range. - Projectile and target destruction do not
create exceptions. - Currency is correct after kills.

### Milestone 4 --- Waves and game states

Tasks: - Spawn one wave of 10 enemies. - Track spawn and resolution
counts. - Add start, pause/resume, restart, win, and lose flows.
Acceptance: - No overlapping duplicate wave. - Wave completion waits for
all spawned enemies to resolve. - Restart returns the game to the
initial state. - Pause reliably stops gameplay.

### Milestone 5 --- HUD and visual polish

Tasks: - Implement currency, base health, wave state, buttons,
selection, and game-over/victory UI. - Add readable materials, simple
effects, and basic audio placeholders only if readily available.
Acceptance: - All required controls work. - UI remains readable at 16:9
and a narrower browser window. - Gameplay remains visually legible.

### Milestone 6 --- WebGL readiness

Tasks: - Check Build Profiles / WebGL platform configuration for Unity
6.6. - Resolve unsupported APIs and build errors. - Build WebGL if the
environment permits. Acceptance: - WebGL build completes, or Claude
reports the exact blocker and gives manual steps. - Never claim success
without a real build result.

### Milestone 7 --- Stabilization and handoff

Tasks: - Fix compile errors and high-priority warnings. - Inspect
console/test output where available. - Add `README.md` with setup,
controls, scene generation, test instructions, and build instructions. -
Summarize files changed and remaining known issues. Acceptance: -
Project can be opened by the user and tested using clear instructions. -
All incomplete features and unverified claims are listed honestly.

Do not jump directly to advanced content. Finish and verify each
milestone before expanding scope.

------------------------------------------------------------------------

## 12. Test checklist

At minimum, test or give explicit manual steps for each item: - \[ \]
Project opens without compile errors. - \[ \] Prototype scene is
created/saved and opens reliably. - \[ \] Camera shows entrance, path,
base, and all tower pads. - \[ \] Enemies follow the route in the
correct order. - \[ \] Enemies reach the base and reduce health exactly
once. - \[ \] A tower can be purchased when the player has enough
currency. - \[ \] A tower cannot be purchased when currency is
insufficient. - \[ \] An occupied pad cannot receive a duplicate
tower. - \[ \] Towers attack only enemies within range. - \[ \]
Projectiles damage targets and handle missing targets safely. - \[ \]
Defeated enemies reward currency exactly once. - \[ \] One wave spawns
exactly 10 enemies. - \[ \] Wave completion waits for all enemies to be
defeated or reach the base. - \[ \] Pause and resume work. - \[ \]
Restart clears spawned objects and resets all game state. - \[ \]
Victory and defeat states are reachable and correctly displayed. - \[ \]
No repeated exceptions occur in the Console. - \[ \] WebGL build is
tested if the environment supports it.

------------------------------------------------------------------------

## 13. Git and change management

-   Check whether the project is already a Git repository before
    initializing it.
-   If Git is not initialized, create a `.gitignore` suitable for Unity,
    excluding generated folders such as `Library/`, `Temp/`, `Obj/`,
    `Build/`, `Builds/`, `Logs/`, and user-specific IDE files as
    appropriate.
-   Keep `Assets/`, `Packages/`, `ProjectSettings/`, and documentation
    under version control.
-   Do not commit huge generated build outputs unless explicitly
    requested.
-   After each milestone, provide a concise summary and recommend a
    checkpoint commit, but do not commit automatically unless the user
    has authorized it.
-   Never remove or overwrite the user's existing work without
    explaining the impact.

------------------------------------------------------------------------

## 14. WebGL-specific notes

-   Use URP-compatible materials and shaders.
-   Prefer simple lighting and modest shadow settings.
-   Avoid huge textures, excessive transparent particles, expensive
    screen-space effects, and excessive real-time lights.
-   Do not rely on local filesystem paths or desktop-only APIs.
-   Keep UI usable with mouse and keyboard in a browser.
-   Check browser console and Unity build output if a build is
    available.
-   Do not add online multiplayer, accounts, ads, analytics, cloud
    saves, or network dependencies in the prototype.
-   If WebGL is not available in the current Unity installation, report
    it and provide the exact Unity Hub module steps rather than silently
    changing the target.

------------------------------------------------------------------------

## 15. Scope boundaries

### Must be in the first playable prototype

-   One level.
-   Winding route.
-   At least 3 tower pads.
-   One enemy type.
-   One archer tower.
-   Currency and base health.
-   One wave of 10 enemies.
-   HUD.
-   Start, pause/resume, restart.
-   Win/lose states.
-   WebGL readiness.

### Explicitly postponed

-   Full mythological hero roster.
-   Complex 3D character animation.
-   Inventory, equipment, skill trees, hero movement.
-   Procedural map generation.
-   Multiple levels and campaign.
-   Save/load system.
-   Online features, accounts, monetization, ads.
-   Paid asset packs.
-   Advanced shaders and heavy VFX.
-   Mobile touch optimization.
-   Complex audio production.

Do not implement postponed features unless the core prototype is stable
and the user asks for expansion.

------------------------------------------------------------------------

## 16. Final instruction to Claude

Read this document and inspect the actual repository. Then execute the
milestones autonomously as far as the available environment allows.

Do not merely provide code snippets in chat: create and edit the
necessary files in the project, use Unity-compatible editor tooling when
needed, run available checks, and continue to the next milestone when
safe.

If a step requires the user to interact with Unity Hub or Unity Editor,
pause only at that specific point and give short, exact instructions.
Otherwise, continue independently.

At the end of each milestone, report: 1. What was implemented. 2. Files
created/changed. 3. Tests/builds actually run and their results. 4.
Known issues or steps requiring manual action. 5. The next milestone.

**Priority order:** working gameplay → correctness and stability → clear
UI → visual polish → extra content.
