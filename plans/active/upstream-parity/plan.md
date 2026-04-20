# Flecs.NET Upstream Parity & .NET 10 Upgrade

## Problem
Flecs.NET wraps flecs v4.0.4 but upstream is at v4.1.5 (6 releases behind, released 2026-03-15). The wrapper also targets .NET 9 while .NET 10 LTS is fully released. Users miss DontFragment storage, OrderedChildren, the on_replace hook, new Entity/World/Table APIs, and carry dead Union code.

## Goals
- Achieve full C++ wrapper feature parity with flecs v4.1.5
- Upgrade target framework to .NET 10 LTS
- Update native submodule, regenerate bindings
- Handle all breaking changes (Union removal, struct field changes)
- Maintain test coverage for new features

## Anti-Goals
- Do NOT wrap HTTP/REST at the high-level (raw bindings + AppBuilder.EnableRest() is sufficient)
- Do NOT expand Meta module beyond what the C++ wrapper provides (struct introspection, full opaque types are out of scope)
- Do NOT expand Script module beyond what the C++ wrapper provides
- Do NOT multi-target net9.0+net10.0 — clean cut to net10.0

## Constraints
- Single feature branch for the flecs upgrade (separate commit for .NET 10)
- Big-bang submodule jump to v4.1.5 (no incremental per-release stepping)
- Must keep existing test suite green (with modifications for breaking changes)
- Code generation patterns must be preserved — new generic types go through Flecs.NET.Codegen

## Audience
- Flecs.NET library consumers (game developers, simulation engineers using ECS in C#)
- Contributors to the Flecs.NET project

## Success Conditions
- All existing tests pass on .NET 10 (modified where needed for breaking changes)
- Native submodule points to v4.1.5 tag
- Bindings regenerated from v4.1.5 headers; version bumped to 4.1.5
- Union tests/examples rewritten to use DontFragment + Exclusive
- All new C++ wrapper features from v4.0.5–v4.1.5 have C# equivalents
- New features have test coverage

---

## Research Notes

### Source 1: Flecs v4.1.0 Release Notes
https://github.com/SanderMertens/flecs/releases/tag/v4.1.0

Key changes: Union removed (replaced by `DontFragment + Exclusive`), non-fragmenting components, OrderedChildren, world exclusive access, Table API expansion, `entity::child()`/`assign()`, single-term observers, bloom filter for queries. Entity id creation in multithreaded mode removed.

### Source 2: Flecs 4.1 Announcement Article
https://ajmmertens.medium.com/flecs-4-1-is-out-fab4f32e36f6

DontFragment is the first open-source ECS to support both archetype and sparse-set storage. Switching is a single trait: `.add(flecs::DontFragment)`. The C++ `get()` API was redesigned to return `const T&` (breaking change); old code migrates via `try_get<T>()`. OrderedChildren preserves child creation order with `set_child_order()`.

### Source 3: Flecs GitHub Discussion #466 — Union Migration
https://github.com/SanderMertens/flecs/discussions/466#discussioncomment-13578339

"Most code should still work as expected after replacing the Union trait with the DontFragment/Exclusive traits." Migration is mechanical: replace `Ecs.Union` with `Ecs.DontFragment` + `Ecs.Exclusive`.

---

## Chosen Approach: Bottom-up — Foundation then Features

1. **.NET 10 first** (separate commit) — change TFM, verify green
2. **Submodule + binding regen** — jump to v4.1.5, regen Flecs.g.cs, fix compile errors
3. **Breaking changes** — Union removal, removed fields, changed signatures
4. **New features** — Layer on wrapper additions incrementally (World, Entity, Table, hooks, traits)

### Why this approach
- Binding regen reveals exactly what changed at the C level
- Many internal improvements (bloom filter, trivial cache, performance) come free from the native update
- Breaking changes are small and contained (Union is 1 alias + 7 tests + 1 example)
- Layering features after a green baseline prevents cascading failures

## Rejected Alternatives

### Incremental per-release stepping (v4.0.5 -> v4.1.0 -> ...)
Rejected because: 6 stops multiplies the regen/fix/test cycle. The wrapper rarely depends on removed intermediate APIs, so big-bang to v4.1.5 is safe.

### Bindings-only (no wrapper additions)
Rejected because: user wants full C++ wrapper parity. Raw bindings exist but the point of Flecs.NET is the high-level API.

### Top-down C++ diff porting
Rejected because: harder to parallelize, and many C++ changes (template metaprogramming, `std::conditional`) have no C# equivalent. Better to work from the feature list.

---

## Acceptance Checks
- [ ] .NET 10 upgrade committed and green
- [ ] native/flecs submodule at v4.1.5
- [ ] Flecs.g.cs regenerated, compiles clean
- [ ] VersionPrefix = 4.1.5
- [ ] `Ecs.Union` alias removed; `Ecs.DontFragment` and `Ecs.Exclusive` aliases added
- [ ] UnionTests.cs rewritten as DontFragmentTests.cs
- [ ] Union example rewritten to use DontFragment + Exclusive
- [ ] `IOnReplaceHook` interface added
- [ ] `World.Shrink()`, `World.Each()`, `World.IdIfRegistered()`, `World.TypeInfo()`, `World.GetVersion()` added
- [ ] `Entity.Child()`, `Entity.Assign()`, `Entity.GetConstant()`, `Entity.OwnsSecond()`, `Entity.AutoOverrideSecond()` added
- [ ] `Entity.Parent()` uses `ecs_get_parent`
- [ ] Table API expanded (Size, Entities, ClearEntities, ColumnSize, Depth, Records, TableId, Lock, Unlock, HasFlags)
- [ ] `Ref.Component()` and untyped references added
- [ ] `Singleton` trait supported
- [ ] `OrderedChildren` trait supported
- [ ] `DontFragment` trait supported
- [ ] Ordered enum/bitmask constants in Meta
- [ ] Observer desc `observable` field handled
- [ ] All new features have test coverage
- [ ] All existing tests pass (modified where needed)
