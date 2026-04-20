# Context: Flecs.NET Upstream Parity

## Key Decisions
- Big-bang jump to v4.1.5 (no incremental stepping)
- Full C++ wrapper parity (not bindings-only)
- .NET 10 as separate commit first
- Single feature branch for the flecs upgrade
- Approach A: bottom-up (foundation then features)

## Version Gap
- Current: flecs 4.0.4 (submodule commit fa9d355760c109c7bfea5b19da5a288eb3acf027)
- Target: flecs 4.1.5 (released 2026-03-15)
- .NET: 9.0 -> 10.0 LTS (10.0.6 released 2026-04-14)

## Union -> DontFragment Migration
Per GitHub discussion #466: "Most code should still work after replacing Union with DontFragment/Exclusive."
- `Ecs.Union` -> `Ecs.DontFragment` + `Ecs.Exclusive`
- Affected files: `Ecs/Aliases.cs` (1 alias), `UnionTests.cs` (7 tests), `Relationships/Union.cs` (1 example)

## C++ get() API Redesign (v4.1.0)
- `get<T>()` now returns `const T&` instead of `const T*`
- New `try_get<T>()` returns `const T*` (nullable)
- C# wrapper already returns refs/values so impact may be minimal, but need to evaluate if TryGet pattern should be added

## New Hook: on_replace (v4.1.1)
- New lifecycle hook between OnRemove and destruction
- Need `IOnReplaceHook` interface following existing pattern in Core/Hooks/

## DontFragment (v4.1.0)
- Non-fragmenting component storage (sparse-set style)
- Adding/removing DontFragment component doesn't change entity's table
- Registration: `.Add(Ecs.DontFragment)` on component entity

## OrderedChildren (v4.1.0)
- Preserves child creation order in hierarchies
- `set_child_order(entities, count)` for reordering

## Non-fragmenting Hierarchy Storage (v4.1.5)
- `Parent` relationship for non-fragmenting hierarchies
- `world.entity(flecs::Parent{p}, "child")` syntax in C++

## Resume Here
All acceptance checks addressed. Remaining: runtime test verification (requires native submodule init + Zig build fix) and committing.

## Discoveries
- .NET 10 upgrade was clean — zero compile errors, zero .NET 10-specific test failures
- Native submodule is not initialized locally (empty directory). Must init before building natives.
- Zig native build has a separate issue with macOS system symbol resolution — may need Zig toolset version update alongside submodule bump
- SourceLink warnings about missing `.git` in submodule are pre-existing, not new
- Bindgen.NET has a bug with function pointer InlineArray types — generates invalid struct names using raw `delegate*` signature. Fixed manually in Flecs.g.cs by naming the struct `ecs_vector_function_callback_t_18`.
- `ecs_ensure_id` gained a mandatory `size` parameter in v4.1.5 — typed calls use `Type<T>.Size`, untyped calls use `ecs_get_type_info(world, id)->size`
- `ecs_iter_t.group_id` field removed — replaced by `ecs_iter_get_group(it)` function
- `ecs_query_iter_t.query` field removed — use `ecs_iter_t.query` directly instead of `priv_.iter.query.query`
- `EcsPrivate` constant removed entirely in v4.1.0 — no direct replacement
- `ecs_world_info_t` lost `systems_ran_frame` and `observers_ran_frame` — per-frame stats moved to `EcsWorldSummary`
- `ecs_script_run` gained a `result` parameter (can pass null)
- `EcsExclusive` already existed as an alias — no need to add it for Union migration
- on_replace hook uses `ecs_iter_action_t` (same as on_add/on_set/on_remove) — exact same callback pattern
- `EcsSingleton` and `EcsOrderedChildren` were already in the regenerated bindings — only needed aliases
- `ecs_set_child_order` and `ecs_get_ordered_children` bindings already generated — Entity wrapper needed
- World.Each() and World.IsAlive() already existed in the wrapper — no work needed
- Entity.OwnsSecond() and Entity.AutoOverrideSecond() already existed — no work needed
- Entity.Parent() was already implemented via Target(EcsChildOf), updated to use ecs_get_parent() directly
- `ecs_cpp_assign` binding exists and is used by Entity.Assign() — handles deferred mode and modified notification
- `ecs_new_w_parent` binding exists for creating named child entities
- Table already had ColumnSize() and Depth() — only 8 new methods needed, not 10
- `flecs_table_records` and `flecs_table_id` are private API functions but are exported and available in bindings
- C++ `untyped_ref` is the base class for `ref<T>` — in C# we implemented as a separate `UntypedRef` struct since C# structs don't support inheritance
- Observer `observable` field is on `ecs_observer_t` (instance), NOT `ecs_observer_desc_t` (descriptor) — no wrapper changes needed
- `EcsConstants` struct already in bindings with `constants` (ecs_map_t*) and `ordered_constants` (ecs_vec_t) fields
- Meta module (Modules/Meta.cs) currently only registers primitives — no struct/enum reflection wrapper
- `Type<T>.RegisterConstants()` already handles enum constant registration
- Ordered enums need: Entity.GetConstants() wrapper + safe ecs_vec_t iteration for ordered_constants
- String type support in Meta.InitModule is still a TODO in the code
- C++ wrapper does NOT expose EcsConstants directly — constants are iterated via entity.children(), same as C# Entity.Children()
- Meta "ordered enums" acceptance check is satisfied by existing infrastructure: Type<T>.RegisterConstants() + Entity.Children() + ToConstant<T>()
- Observer desc `observable` field acceptance check is N/A — field is on ecs_observer_t instance, not descriptor
- C++ `id_if_registered<T>()` checks `_::type<T>::registered(world_)` and returns id or 0 — C# equivalent uses `Type<T>.IsRegistered(Handle, out ulong id)`
- Verification pass against acceptance checks caught two gaps: file rename and missing method

## Files That Will Change
- All 7 `.csproj` files (TFM update)
- `Directory.Build.props` (VersionPrefix)
- `native/flecs` submodule
- `Flecs.g.cs` and `FlecsExtensions.g.cs` (binding regen)
- `Ecs/Aliases.cs` (Union -> DontFragment/Exclusive)
- `Core/Hooks/` (new IOnReplaceHook.cs)
- `Core/World.cs` (Shrink, Each, IdIfRegistered, TypeInfo, GetVersion)
- `Core/Entity.cs` (Child, Assign, GetConstant, Parent rewrite, OwnsSecond, AutoOverrideSecond)
- `Core/Table.cs` (10+ new methods)
- `Core/Ref.cs` (Component method, untyped refs)
- Tests and examples (Union -> DontFragment rewrites + new feature tests)
- Generated code (if codegen generators change)
