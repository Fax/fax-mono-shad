# Fax Mono Shad – Architecture Overview

This document explains the structure, data flow, and key patterns of the project. It’s meant for contributors who want to understand how systems interact, how to extend the game, and where to improve performance and maintainability.

## High-level summary

- Runtime: .NET 8 with MonoGame DesktopGL and MonoGame.Extended.
- Pattern: Simple game loop (Game1) + event-driven messaging (EventBus) + lightweight managers/renderers.
- World: Player + Enemies + Bullets + Pickups + Experience + transient AreaEffects.
- Rendering: SpriteBatch with a camera for the world, then a second pass for UI.
- Content: Procedural 1x1 color textures; dynamic SFX generated in code.

## Key assemblies and dependencies

- MonoGame.Framework.DesktopGL (3.8.*): core game loop, rendering, input.
- MonoGame.Extended (5.0.2): camera and viewport adapters.
- MonoGame.Content.Builder.Task (3.8.*): content pipeline (currently minimal usage; most content is procedural).

## Core loop

The entry point (`Program.cs`) constructs `Game1` and calls `Run()`. `Game1` is the orchestrator:

- Initialize: sets up graphics, content root, input, texture manager, camera.
- LoadContent: instantiates managers, renderers, spawners, player, and collections.
- Update (per frame):
  1. EventBus.Drain() handles queued events from the previous frame.
  2. Camera and input are updated; pause toggled; exit checks.
  3. Player updates (movement/dash/weapon firing).
  4. Managers/spawners update: bullets, pickups, area effects, enemies, experience.
  5. Immediate collision checks (bullet vs enemy, area effects vs enemy, player vs pickups, player vs experience range and collection).
  6. New events are published into the EventBus for next frame’s `Drain()`.
- Draw (per frame):
  - Begin world pass with camera transform; draw player, area effects, bullets, pickups, experience, enemies; End.
  - Begin UI pass (no transform); render HUD bars; End.

This produces a predictable, single-threaded frame pipeline with inter-frame event processing.

## Messaging (EventBus)

- `EventBus` provides pub/sub with a queue of `IGameEvent`.
- `Publish<T>(evt)` enqueues events; `Drain()` dequeues all and synchronously invokes subscribers.
- Subscribers are stored per event type and invoked via `Delegate.DynamicInvoke`.
- Usage examples:
  - `BulletCollisionEvent` and `AreaCollisionEvent` reduce enemy health; on death, enemies publish `SpawnExperienceEvent` and play SFX.
  - `ShootEvent` triggers bullet creation in `BulletSpawner`.
  - `WeaponPickupEvent` updates `Weapon.BulletType` and removes the pickup.
  - `CollectExperienceEvent` increases player experience and may publish `NextLevelEvent`.

Notes:
- Events published within a frame will be processed on the next frame (because `Drain()` is called at the start of `Update`). This gives a stable ordering but is worth remembering when adding time-sensitive logic.

## Entities and data model

- Player (`Player`): position, rotation, size, movement/dash state, score/experience/level, weapon.
- Weapon (`Weapon`): cooldown timer, current `BulletType`; converts input into `ShootEvent` and plays SFX.
- Bullet (`Bullet` and subclasses): position, velocity, life, speed, friction, color, damage, splash params, unique `EntityId`.
- Enemy (`EnemyEntity`): position, life/max life, size, color, speed, active flag.
- Pickup (`PickupEntity` + `WeaponPickup`): position, size, rotation, color; carries a `BulletType`.
- Experience (`ExperienceEntity`): amount, active flag, position, attraction speed; color derived from amount.
- AreaEffect (`AreaEffect`): center, range, color, damage, collider/render flags, duration.
- IDs: `IdManager` creates unique integer IDs used in events and entity lookup.

## Systems (Managers / Spawners / Renderers)

- InputManager: merges keyboard+mouse into an `InputState` with derived pulses/holds; transforms mouse coordinates via camera.
- EnemyManager: spawns enemies at intervals around a ring, moves them toward player, applies bullet/area damage via events, and on death publishes `SpawnExperienceEvent`.
- BulletSpawner: listens for `ShootEvent`, creates bullet instances, updates life/velocity/position, publishes `SplashDamageEvent` on expiration when applicable, and removes dead bullets.
- PickupSpawner: periodically clears and spawns a single `WeaponPickup` from the `BulletRegistry` selection; removes it upon `WeaponPickupEvent`.
- PickupManager: rotates pickups.
- ExperienceManager: spawns nuggets on events and moves attracted nuggets toward the player while accelerating them; clears collected ones.
- AreaEffectManager: spawns transient areas on `SplashDamageEvent`; counts down their duration; draws circles (scaled square texture) and provides bounding boxes for collision.
- CollisionManager: AABB checks only; used directly in `Game1` for bullet/enemy, area/enemy, player/pickup, player/experience.
- TextureManager: creates 1x1 textures colored per logical sprite key and provides access by key.
- Renderers: one per category (player, bullet, enemy, pickup, experience, UI). UI has helpers for generic horizontal bars.

## Rendering and camera

- Uses `BoxingViewportAdapter` and `OrthographicCamera` from MonoGame.Extended for a letterboxed camera with zoom.
- World pass uses `transformMatrix` from the camera; UI pass renders in screen space.
- Entities are drawn from simple 1x1 textures scaled to rectangles; `PlayerRenderer` applies rotation around texture center.

## Input and player control

- WASD/arrow keys for movement.
- Mouse for aiming; left click produces `ShootEvent` pulses.
- Space produces a dash pulse; dash state machine accelerates player for a short time and applies a cooldown bar in the UI.

## Leveling and UI

- `Levels.NextLevelExperience(level)` returns required XP as 1000 * 1.8^level.
- The UI shows an experience bar (progress within current level) and a dash cooldown bar.

## Audio

- `Sfx` generates and caches three dynamic sounds at startup: shoot, explosion, and pling.
- Sounds are mixed/generated procedurally using `SoundGenerator` with simple waveforms and noise.

## Data flow per frame (simplified)

1. EventBus.Drain(): process previous frame’s events → mutate state via managers/subscribers.
2. InputManager.Update(): populate current input + mouse world pos.
3. Player.Update(): movement/aim; fire weapon → Publish ShootEvent.
4. Spawners/Managers Update():
   - BulletSpawner: create bullets from ShootEvent; integrate bullets; expire → SplashDamageEvent.
   - EnemyManager: spawn/move enemies; respond to BulletCollisionEvent/AreaCollisionEvent; death → SpawnExperienceEvent.
   - PickupSpawner: time-based spawn; remove on WeaponPickupEvent.
   - PickupManager: rotate pickups.
   - ExperienceManager: move attracted nuggets; remove collected ones.
   - AreaEffectManager: decay areas.
5. Collisions in Game1:
   - For each bullet vs each enemy → publish BulletCollisionEvent.
   - For each active area vs each enemy → publish AreaCollisionEvent.
   - Player vs pickups → publish WeaponPickupEvent.
   - Player vs experience: proximity acceleration and direct collection → publish CollectExperienceEvent.
6. Draw world (with camera), then Draw UI.

## Notable implementation details

- Event ordering: Publishing during Update schedules handlers for the next frame; this avoids reentrancy during the same frame but introduces one-frame latency for reactions.
- Removal: many lists use `RemoveAll` post-Update per system to prune inactive/dead items.
- Physics: simple Euler integration with no collision response (only triggers), and friction modeled as exponential decay on velocity.

## Known issues and technical debt (as of this snapshot)

- BulletRegistry.Instance getter bug:
  - Current code: `if (_instance != null) _instance = new BulletRegistry();` will re-create the instance when it is not null, and returns null otherwise. It should likely be `if (_instance == null) _instance = new BulletRegistry(); return _instance;`.
  - Consequences: `BulletRegistry` may not initialize or may thrash.
- InputManager hold-state bugs:
  - `current.RightHold = current.RightHold && previous.RightHold;` and similarly for `MiddleHold` reference themselves instead of the click states. They should compare `current.RightClick && previous.RightClick` and `current.MiddleClick && previous.MiddleClick`.
- Vector2Extensions.ToCenteredBoundingBox overload has unreachable code:
  - It returns early with a centered rectangle and then contains dead code computing a different rectangle. Needs cleanup and a single, correct implementation.
- UI bar fill texture inconsistency:
  - `UIHelpers.RenderHorizontalBar` uses `"bar"` for both background and fill; a `"barfill"` texture is initialized in `Game1.LoadContent` but isn’t used here. Consider switching the fill to `"barfill"` for clarity.
- BulletRegistry.SelfRegister robustness:
  - Uses reflection to instantiate all types assignable from `Bullet`; needs guards for abstract types or explicit registration from data to avoid unexpected activations.
- EventBus performance and diagnostics:
  - Uses `DynamicInvoke` and writes to console per invocation; consider strongly-typed lists and direct `Action<T>` invocation to reduce allocations/reflection and remove noisy logging in release.
- Collision complexity:
  - Bullet vs enemy and area vs enemy checks are O(B×E). Consider spatial partitioning (grid/quad-tree) if entities grow.
- Threading:
  - All systems are currently single-threaded, which simplifies state management. If you add background loading, keep EventBus and entity lists confined to the main thread or add synchronization.

## Extending the game

- Add a new bullet type:
  1. Create a subclass of `Bullet` with desired parameters (e.g., color, speed, friction, damage, splash).
  2. Register it in `BulletRegistry` (either via `Register(new MyBullet())` or a data-driven registry once implemented).
  3. Optionally update `BulletSpawner.CreateBullet` to map a `BulletType` to a concrete class if not using the registry lookup yet.
  4. Add a matching `WeaponPickup` color mapping if needed.

- Add a new event-driven system:
  1. Define an `IGameEvent` struct in `Events/`.
  2. Subscribe to it in a manager’s constructor.
  3. Publish it from game logic or other managers.

- Add a new pickup type:
  1. Subclass `PickupEntity` for custom behavior.
  2. Render via `PickupRenderer` or a specialized renderer.
  3. Handle collection in `Game1.Update` collision loop or move that logic into a dedicated manager.

## Testing and debugging hints

- Toggle pause with P; use console output currently present in EventBus to trace handlers.
- For tuning, `Cooldown`, `Speed`, `Friction`, and `Duration` fields are public and easy to manipulate.
- Use MonoGame.Extended camera zoom (mouse wheel) to inspect world elements.

## Potential improvements

- Correct the `BulletRegistry.Instance` getter and wire a proper registry-based spawn path in `BulletSpawner`.
- Fix input hold-state logic; add unit tests for `InputManager.Update` and common edge cases.
- Extract collision detection into a dedicated system and use spatial hashing for scalability.
- Replace `DynamicInvoke` with strongly typed handler lists (`Dictionary<Type, List<Action<T>>>` via generics) to avoid reflection.
- Data-driven bullets and pickups: load from JSON or content pipeline; avoid reflection scanning.
- Pool bullets and enemies to reduce allocation churn.
- Guard against invalid camera zoom extremes; clamp zoom.
- Move collision checks for pickups/experience into their respective managers to keep `Game1.Update` focused.

## File map (representative)

- Root: `Game1.cs`, `Program.cs`, `EventBus.cs`, `GlobalUsing.cs`.
- Entities: `Bullet.cs`, `Enemy.cs`, `Experience.cs`, `Pickup.cs`, `Weapon.cs`.
- Events: `ShootEvent.cs`, `BulletCollisionEvent.cs`, `AreaCollisionEvent` (in same file), `SpawnExperienceEvent.cs`, `CollectExperienceEvent` (same file), `WeaponPickupEvent.cs`, `ScoreEvent.cs`, `NextLevelEvent.cs`, `SplashDamageEvent.cs`.
- Managers: `EnemyManager.cs`, `AreaEffectManager.cs`, `ExperienceManager.cs`, `PickupManager.cs`, `InputManager.cs`, `TextureManager.cs`, `AudioManager.cs` (Sfx).
- Spawners: `BulletSpawner.cs`, `PickupSpawner.cs`, `ISpawner.cs`.
- Renderers: `PlayerRenderer.cs`, `BulletRenderer.cs`, `EnemyRenderer.cs`, `PickupRenderer.cs`, `ExperienceRenderer.cs`, `UIRenderer.cs` (+ `UIHelpers`).
- Utils: `Extensions.cs` (Color/Vector helpers), `RectangleExtensions.cs`, `Stuff.cs` (CollisionManager, Levels), `ShaderSource.cs`.

## Constraints and assumptions

- Single-threaded update/render; events are deferred by one frame.
- 2D AABB collisions; no physics engine.
- Procedural textures and SFX; minimal dependency on external assets.
- Coordinates follow MonoGame screen space (Y increases downward). Camera handles world-to-screen mapping.

---

Last updated: 2025-09-05.
