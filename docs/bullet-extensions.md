# Bullet Behaviors: Architecture Options and Plan

These abstractions let you implement bouncy, boomerang, chain, explosive, piercing, splitting, orbiting, and other behaviors without changing the main loop.

## Option B example — minimal ECS for bullets (exploratory)

This section outlines a lightweight ECS-style approach focused only on bullets. It’s additive and can coexist with the current code (no sweeping refactor). The goal is to compose behaviors like Bounce + Homing + ExplodeOnExpire by attaching multiple components and letting systems process them in sequence.

### Objectives

- Compose bullet behavior via small components (Bounce, Homing, Lifetime, etc.).
- Keep systems simple, ordered, and easy to reason about.
- Avoid touching enemies/pickups initially; only bullets use ECS in this exploration.

### Requirements to support multi-component bullets

1) Entity identity
  - Each bullet has a stable `int EntityId` (reuse `IdManager.NextId()`).

2) Component storage
  - Per-component dictionaries keyed by `EntityId` (e.g., `Dictionary<int, Bounce>`, `Dictionary<int, Homing>`).
  - Components are small records/structs with only data; no logic.

3) Systems
  - Small classes with `Update(dt, ctx)` methods iterating required component sets.
  - Examples: `BulletLifetimeSystem`, `BulletMovementSystem`, `BulletBounceSystem`, `BulletHomingSystem`, `BulletExpireSystem`.

4) Update order contract (per frame)
  - Lifetime → Behavior (Homing, Bounce pre-adjustments) → Movement/Integration → Collision resolution → Expire.
  - Keep ordering explicit in a `BulletEcs.Update(dt)` orchestrator to avoid surprises.

5) World/context API
  - Query helpers: `IEnumerable<(int id, EnemyEntity enemy)> GetEnemiesInRange(Vector2 center, float radius)`.
  - Publish events: wraps existing `EventBus` for splash, score, etc.
  - Bounds info and RNG.

6) Bridging collisions
  - Continue detecting bullet–enemy collisions in `Game1.Update` (as today) but forward them to the ECS collision system: `BulletCollisionSystem.RegisterHit(bulletId, enemyId, contactPoint)`.
  - The collision system consumes registered hits during its update and applies component-driven reactions (bounce, chain, pierce) instead of unconditionally killing bullets.

7) Spawn/despawn lifecycle
  - On `ShootEvent`, create a bullet entity and attach a base set of components: `Transform`, `Velocity`, `Lifetime`, `Damage`, optional `Sprite`.
  - Attach behavior components as needed: `Bounce{Max, Restitution}`, `Homing{Radius, Strength}`, `ExplodeOnExpire{Damage, Radius}`, etc.
  - Despawn removes the entity from all component stores.

### Data contracts (component sketches)

- Transform: `Vector2 Position`
- Velocity: `Vector2 Value`
- Lifetime: `float Remaining`
- Damage: `float Amount`
- Bounce: `int Remaining; float Restitution` (0..1)
- Homing: `float Radius; float TurnRate` (rad/s) or `float Acceleration`
- ExplodeOnExpire: `float Damage; float Radius`
- Owner: `int PlayerId` (optional)
- Visual/Sprite: `Color Tint; Vector2 Size`

Store example: `Dictionary<int, Transform> Transforms`, etc.

### Systems (behavior and responsibilities)

- BulletLifetimeSystem: decrement `Lifetime.Remaining`; mark for expire when ≤ 0.
- BulletHomingSystem: for entities with (Transform, Velocity, Homing), find nearest enemy within radius, steer toward target (limit by `TurnRate` or add force toward target), no teleport or overshoot.
- BulletMovementSystem: integrate `Transform.Position += Velocity.Value * dt * speedFactor` and apply friction if desired.
- BulletCollisionSystem: consume collisions registered for this frame; for entities with `Bounce`, reflect velocity around normal; decrement `Bounce.Remaining`. If no `Bounce` or exhausted, mark for removal. Always apply damage to enemy via EventBus (e.g., reuse `BulletCollisionEvent` or a new `BulletHitEvent` directly handled by `EnemyManager`).
- BulletExpireSystem: on removal due to lifetime or velocity threshold, if `ExplodeOnExpire` present, publish `SplashDamageEvent` then despawn.

Suggested system order inside `BulletEcs.Update(dt)`:
1) Lifetime
2) Homing
3) Movement
4) Collision (consumes hits from Game1)
5) Expire (and cleanup)

### Minimal world API (context)

Define a `BulletWorldContext` passed to systems:
- `EventBus Bus`
- `Func<Vector2, float, IEnumerable<EnemyEntity>> GetEnemiesInRange`
- `RectangleF WorldBounds` (or just viewport size)
- `Random Rng`

### Spawn pipeline (example)

ShootEvent → BulletFactory:
- Create `entityId = IdManager.NextId()`.
- Add components: `Transform(Position = origin)`, `Velocity(Value = dir * speed)`, `Lifetime(Remaining = baseLife)`, `Damage(Amount = baseDamage)`, `Visual(Tint = color, Size = baseSize)`.
- If type is Bouncy: add `Bounce(Remaining = maxBounces, Restitution = 0.8f)`.
- If type has Homing: add `Homing(Radius = 250, TurnRate = X or Acceleration = Y)`.
- If type explodes on expire: add `ExplodeOnExpire(Damage, Radius)`.

### Example: Bouncy + Homing composition (pseudo)

Composition:
- Components: Transform, Velocity, Lifetime(2.0s), Damage(8), Bounce(3, 0.7), Homing(radius: 220, acceleration: 600), Visual(bulletTint, size: 10×10)

Behavior across systems:
- Homing system gently bends velocity toward nearest enemy if within 220px.
- Movement integrates new velocity.
- Collision system: on hit, reflect velocity = `reflect(v, normal) * restitution`, `Bounce.Remaining--`. If `Remaining` < 0 → mark for removal. Damage is still applied on every hit.
- Expire system: if lifetime runs out or speed below threshold, remove (and optionally explode if ExplodeOnExpire present).

### Bridging with current code (no global refactor)

- Keep `Game1.Update` collision detection as-is, but instead of publishing `BulletCollisionEvent` directly to kill bullets, forward hits to the ECS: `BulletEcs.Collisions.Add((bulletId, enemyId, contactPoint))`.
- In `BulletSpawner.Update(dt)`, stop integrating ECS-managed bullets; delegate integration to `BulletEcs.Update(dt)`. You can maintain a separate list for ECS bullets (e.g., `EcsBulletSet`) while keeping legacy bullets unaffected.
- Rendering: iterate ECS bullet entities (those with Transform + Visual) in `Draw` using existing `BulletRenderer` by translating component data to the renderer call.

### Risks and performance notes

- Start with dictionary lookups; add simple indexing or a spatial grid only if needed.
- Avoid excessive allocations in per-frame queries; reuse buffers for `GetEnemiesInRange`.
- Keep systems deterministic where possible; seed `Random` with bullet id if necessary.

### Next steps (exploration checklist)

- [ ] Create `BulletEcs` container with component stores and system list.
- [ ] Implement Lifetime, Homing, Movement, Collision, Expire systems.
- [ ] Add a small `BulletFactory` translating current bullet types to component sets.
- [ ] Wire shooting to create ECS bullets for the Bouncy type only (others remain legacy).
- [ ] Bridge collisions from `Game1` to `BulletCollisionSystem` and verify bounces keep bullets alive.
- [ ] Render ECS bullets via existing renderer.

This ECS slice for bullets can later be expanded to piercing, splitting, boomerang, chain lightning, and orbiting by introducing small, focused components and corresponding systems without modifying the main loop.

### Attachment, storage, retrieval, and influence (the missing pieces)

Where do you attach components?
- At spawn time in a `BulletFactory` reacting to `ShootEvent`. That’s the primary place to compose behaviors from bullet type/data.
- Dynamically at runtime when rules apply (e.g., pickup grants homing): a system or event handler can call `ecs.Add(entityId, new Homing { ... })`.

How are components stored?
- One central `BulletEcs` holds per-component stores, each a dictionary keyed by `entityId`:
  - `Dictionary<int, Transform> Transforms`
  - `Dictionary<int, Velocity> Velocities`
  - `Dictionary<int, Lifetime> Lifetimes`
  - `Dictionary<int, Bounce> Bounces`, `Dictionary<int, Homing> Homings`, etc.
- Entity membership is implicit: an entity “has” a component if it appears in that component’s dictionary.
- Removal/despawn deletes the `entityId` from all stores.

How do systems retrieve components (joins/views)?
- Provide typed "views" that iterate entities that have all required components. Implementation pattern:

  Pseudo-API
  - `IEnumerable<(int id, ref T1, ref T2)> View<T1, T2>()`
  - `IEnumerable<(int id, ref T1, ref T2, ref T3)> View<T1, T2, T3>()`
  - The view iterates the smallest backing dictionary among the requested components and `TryGetValue` the others. Yield references or mutable wrappers so systems can update in place.

  Example (HomingSystem)
  - `foreach (var (id, ref Transform tr, ref Velocity vel, ref Homing hom) in ecs.View<Transform, Velocity, Homing>()) { /* steer vel based on nearest enemy */ }`

How do components influence each other?
- Influence is realized via systems writing to components that other systems read later in the same frame, governed by a strict update order. Suggested ownership and order:
  1) LifetimeSystem (writes: Lifetime.Remaining, removal flags)
  2) HomingSystem (reads: Transform, Homing; writes: Velocity)
  3) MovementSystem (reads: Velocity; writes: Transform)
  4) CollisionSystem (consumes hits; reads: Bounce, writes: Velocity and bounce counters; may mark for removal; publishes damage)
  5) ExpireSystem (reads: Lifetime and removal flags; if ExplodeOnExpire present → publishes SplashDamageEvent; then despawns)
- Rule of thumb: one system is the sole writer for a given concern within a frame to avoid conflicts (e.g., only HomingSystem modifies Velocity pre-move; CollisionSystem may override Velocity after a hit).

Concise reference pseudo-API
- `int Create()` — returns new entity id.
- `void Add<T>(int id, T component)` / `bool Remove<T>(int id)` / `ref T Get<T>(int id)` / `bool TryGet<T>(int id, out T c)`.
- `IEnumerable<(int id, ref T1, ref T2, ...)> View<T1, T2, ...>()` — joined iteration.
- `void Despawn(int id)` — removes from all component stores.
- `void RegisterHit(int bulletId, int enemyId, Vector2 contactPoint)` — queued for `CollisionSystem`.

Worked micro-example
- Spawn bouncy-homing bullet:
  1) `id = ecs.Create()`
  2) `ecs.Add(id, new Transform { Position = origin })`
  3) `ecs.Add(id, new Velocity { Value = dir * speed })`
  4) `ecs.Add(id, new Lifetime { Remaining = 2.0f })`
  5) `ecs.Add(id, new Damage { Amount = 8 })`
  6) `ecs.Add(id, new Bounce { Remaining = 3, Restitution = 0.7f })`
  7) `ecs.Add(id, new Homing { Radius = 220, Acceleration = 600 })`
  8) `ecs.Add(id, new Visual { Tint = color, Size = new(10,10) })`
- Frame N:
  - HomingSystem bends `Velocity` toward nearest target.
  - MovementSystem integrates `Transform` from `Velocity`.
  - Game1 detects hit → calls `ecs.RegisterHit(id, enemyId, contactPoint)`.
  - CollisionSystem reflects `Velocity` using contact normal, decrements `Bounce.Remaining`; applies damage via EventBus; if bounces exhausted → mark for removal.
  - ExpireSystem checks `Lifetime.Remaining` and removal flags; publishes splash if needed; `ecs.Despawn(id)` when done.

This clarifies the exact points of attachment, storage/retrieval mechanics, and how component updates influence one another deterministically across systems.

This document proposes ways to add rich, pluggable bullet behaviors (e.g., bouncy, boomerang, chain lightning, varied explosions) and how to evolve the current code with minimal disruption.

## Goals and constraints

- Add new bullet behaviors without touching core loops every time.
- Keep simple behaviors cheap and easy to add.
- Support per-bullet tuning (speed, friction, bounce count, homing radius, etc.).
- Integrate cleanly with existing EventBus and managers.
- Incremental refactor: first enable a Bouncy bullet; later extend to Boomerang and Chain Lightning.

## Current snapshot (relevant)

- `BulletSpawner` owns bullet list and updates movement/life; kills bullets on collision via `BulletCollisionEvent`.
- `Game1.Update` detects bullet–enemy collisions and publishes events.
- `AreaEffectManager` handles splash damage via `SplashDamageEvent`.
- `Bullet` is data-only (position/velocity/life/damage/etc.).

## Option A — Strategy pattern (per-bullet behavior)

Add a behavior contract implemented by each bullet type. Each `Bullet` holds an `IBulletBehavior` that implements lifecycle hooks.

Contract (suggested):

- OnSpawn(context, bullet)
- Update(context, bullet, dt): may modify position/velocity, enqueue events, or mark bullet for removal.
- OnCollision(context, bullet, enemy): decide whether to remove, bounce, chain, explode, etc.
- OnExpire(context, bullet): e.g., trigger area effect.

Context should provide:

- EventBus, random, world queries (e.g., get enemies near position), boundaries, elapsed time.

Examples:

- BouncyTowardNearestEnemyBehavior
  - Fields: maxBounces, restitution, seekRadius
  - OnCollision: reflect velocity across contact normal (approximate from incoming velocity vs. (enemy.pos − bullet.pos)); decrement bounces; optionally adjust direction toward nearest enemy within radius.
  - Remove when bounces == 0 or life <= 0.

- BoomerangBehavior
  - Outgoing phase until max distance or time, then return phase homing to origin/owner.

- ChainLightningBehavior
  - OnCollision: damage primary; then find up to N nearest enemies within hopRadius, publish damage events along hops; optionally spawn a temporary visual area/beam.

Pros
- Minimal code changes; behaviors are cohesive and testable.
- Works with current lists and EventBus.

Cons
- Behavior logic stays inside bullet updates; harder to share cross-cutting logic (e.g., homing + bounce combo) unless you chain behaviors.

## Option B — ECS-style components and systems

Split bullet behavior into composable components; systems iterate and apply logic.

- Components: Movement, Lifetime, Bounce, Homing, Boomerang, ChainLightningEmitter, ExplosionOnExpire, etc.
- Systems: BulletMovementSystem, BulletCollisionSystem, BounceSystem, HomingSystem, ChainLightningSystem.
- `Bullet` becomes a container of components; systems update all bullets.

Pros
- Highly composable; reuse common parts.

Cons
- Bigger refactor; more boilerplate and indirection.

## Option C — Data-driven composition (JSON/YAML)

Define bullets via data that maps to behaviors/components with parameters. Example structure:

- id, name, visuals
- base stats: speed, life, damage
- behaviors: [ { type: "Bounce", maxBounces: 3, restitution: 0.8 }, { type: "SeekNearest", radius: 250 }, { type: "ExplodeOnExpire", radius: 50, damage: 2 } ]

Load definitions at startup; factory builds bullet instances by composing behaviors/components.

Pros
- Designers can tweak without code; quick iteration.

Cons
- Requires a reflection/registry mapping and validation; more plumbing.

## Option D — Scripted behaviors (C# scripts)

Use Roslyn scripting to define `Update`/`OnCollision` in scripts. Pass a limited API (`context`) to ensure safety.

Pros
- Maximum flexibility.

Cons
- Complexity, security, performance, and deployment considerations. Likely overkill for now.

## Option E — Event-chained micro-behaviors

Express bullet reactions as a set of event handlers reacting to granular events (`BulletCollided`, `BulletExpired`, `BulletBounced`, etc.). Compose via subscriptions.

Pros
- Stays within existing EventBus model.

Cons
- Harder to track per-bullet state; risk of fragmented logic.

## Recommendation

Start with Option A (Strategy) as a thin layer, designed to evolve toward Option C (data-driven). If combinatorial behaviors become common, graduate to Option B (components) later.

## Minimal refactor plan (Phase 1: Strategy + Bouncy bullet)

1) Add behavior contracts
- New file `Entities/BulletBehavior.cs`:
  - `interface IBulletBehavior`
  - `readonly struct BulletBehaviorContext` exposing: `EventBus Bus`, `Func<Vector2, float, IEnumerable<EnemyEntity>> GetEnemiesInRange`, screen/world bounds, random.

2) Extend `Bullet`
- Add: `IBulletBehavior Behavior` (nullable for default behavior), `int BounceCount`, `Vector2 Origin`, optional `int OwnerId`.

3) Update `BulletSpawner`
- On spawn: set `bullet.Behavior` based on bullet type or registry mapping.
- In `Update(dt)`: before physics integration, call `Behavior?.Update(ctx, bullet, dt)`; then integrate; clamp and detect OOB bounces if behavior requests.
- On expiration: call `Behavior?.OnExpire(ctx, bullet)` before removal.

4) Collision handling
- In `BulletSpawner` collision handler (`OnCollide`), replace unconditional `b.Life = 0f;` with:
  - If `b.Behavior != null` and `b.Behavior.OnCollision(ctx, b, enemy)` returns `HandledKeepAlive`, don’t kill the bullet; else remove.
- Optionally add a simple contact normal approximation: use incoming velocity and enemy-to-bullet vector.

5) Implement first behaviors
- `BouncyTowardNearestEnemyBehavior`: supports bounce count, restitution, seek radius.
- `ExplodeOnExpireBehavior`: publishes `SplashDamageEvent` on `OnExpire`.

6) Registry mapping
- Expand `BulletRegistry` to map `BulletType` → behavior factory (and base stats). For now, a dictionary is enough.

Deliverable: Bouncy bullets that bounce off enemies instead of exploding, optionally nudging toward the nearest enemy after each bounce.

## Phase 2 — Boomerang and Chain Lightning

- Boomerang: two-phase state inside behavior; track `maxDistance` or `maxTime` then home back to `Origin`/`OwnerId`.
- Chain Lightning: on collision, gather nearest enemies within `hopRadius`, hop `maxHops`, apply damage per hop, and optionally spawn temporary visuals (e.g., reuse `AreaEffect` or add a line renderer).

## Phase 3 — Data-driven definitions

- Add JSON definitions under `Content/Definitions/Bullets/*.json`.
- Implement a factory that parses definitions and composes behaviors.
- Validate on load; expose to `PickupSpawner` so pickups spawn from the same set.

## Key integration points

- `Game1.Update` collision loop: eventually move bullet–enemy collision into a dedicated BulletCollisionSystem so behaviors can request reflection with more context.
- `EnemyManager`: provides queries (e.g., nearest enemies). Consider adding a query helper API.
- `EventBus`: add new events as needed, e.g., `BulletBouncedEvent`, `ChainLightningHitEvent`.

## Edge cases and safeguards

- Infinite bounces: cap `maxBounces` and minimum speed; remove bullet if speed drops below threshold.
- No enemies near homing/chain: gracefully fallback (straight reflection or removal).
- Out-of-bounds: reflect or destroy depending on behavior; clamp positions.
- Performance: avoid O(B×E) scans by adding a simple spatial grid for neighbor queries if counts grow.
- Determinism: if needed, seed behavior-local RNG with bullet id.

## Testing suggestions

- Unit test `IBulletBehavior` implementations in isolation with fake contexts.
- Integration test: spawn a bouncy bullet toward a wall of enemies and assert bounce count and final removal.
- Visual smoke tests: enable a debug toggle to draw intended next target/seek radius.

## Example contracts (sketch)

- IBulletBehavior
  - bool OnCollision(ctx, bullet, enemy) — return true to keep bullet alive (e.g., after bounce), false to let spawner remove it.
  - void OnExpire(ctx, bullet)
  - void Update(ctx, bullet, dt)

- Context services
  - IEnumerable<EnemyEntity> GetEnemiesInRange(Vector2 center, float radius)
  - void Publish<TEvent>(TEvent evt)

These abstractions let you implement bouncy, boomerang, chain, explosive, piercing, splitting, orbiting, and other behaviors without changing the main loop.
