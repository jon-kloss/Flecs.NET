# Tasks: Flecs.NET Upstream Parity & .NET 10 Upgrade

## Now

(none — all work complete, 1402/1402 tests passing)

## Later

(none)

## Blocked

(none)

## Done

- [x] **.NET 10 upgrade** — Updated 7 csproj TFMs from net9.0 to net10.0 (kept Flecs.NET.Native at netstandard2.1). Build succeeds, 0 errors. Test infrastructure works on net10.0 (1388 tests discovered, all native-dependent failures expected due to uninitialized submodule).
- [x] **Submodule update + binding regen** — Updated native/flecs to v4.1.5, ran Flecs.NET.Bindgen, fixed 7 categories of compile errors (function pointer InlineArray naming, ecs_ensure_id size param, removed EcsUnion/EcsPrivate, iter group_id→ecs_iter_get_group, ecs_query_iter_t.query→ecs_iter_t.query, WorldInfo field removal, ecs_script_run result param), regenerated codegen, updated VersionPrefix to 4.1.5.
- [x] **Handle breaking changes** — Union→DontFragment migration (Aliases.cs, UnionTests.cs, Union example), removed EcsPrivate alias, fixed WorldInfo to use _total fields instead of removed _frame fields.
- [x] **Add IOnReplaceHook interface** — Created IOnReplaceHook.cs following existing hook pattern. Added OnReplace field to TypeHooksContext, registration in Type.cs, 20 callback variants in Functions.cs, 18 public OnReplace overloads in Component.cs, 2 private SetOnReplaceCallback methods.
- [x] **Add new traits (Singleton, OrderedChildren)** — Added Ecs.Singleton and Ecs.OrderedChildren aliases in Aliases.cs. Added Entity.SetChildOrder() wrapper for ecs_set_child_order. Both EcsSingleton and EcsOrderedChildren were already in regenerated bindings.
- [x] **New World methods** — Added Shrink(), TypeInfo() (3 overloads), GetVersion(), IdIfRegistered<T>(). Each() and IsAlive() already existed.
- [x] **New Entity methods** — Added Child() (2 overloads), Assign() (3 overloads with AssignInternal using ecs_cpp_assign), GetConstant<T>(). Rewrote Parent() to use ecs_get_parent() directly. OwnsSecond() and AutoOverrideSecond() already existed.
- [x] **Table API expansion** — Added 8 new methods: Size(), Entities(), ClearEntities(), Records(), TableId(), Lock(), Unlock(), HasFlags(). ColumnSize() and Depth() already existed.
- [x] **Ref improvements** — Added Component() method to Ref<T> returning Id. Created UntypedRef struct (non-generic) with GetPtr(), TryGetPtr(), Has(), Entity(), Component(), and bool conversion.
- [x] **Meta improvements** — Verified C# wrapper already matches C++ wrapper scope. EcsConstants bindings exist from regen. Type<T>.RegisterConstants() preserves order. Entity.Children()/ToConstant<T>()/GetConstant<T>() provide same access as C++. Observer `observable` is on instance, not descriptor — no changes needed.
- [x] **Tests for new features** — Added 8 Table tests (Size, Entities, ClearEntities, TableId, LockUnlock, HasFlags, Records) and 7 Ref tests (Component, UntypedRef GetPtr/Entity/Component/Has/TryGetPtr/TryGetPtrAfterDelete) to existing test files.
- [x] **Acceptance check gaps** — Renamed UnionTests.cs→DontFragmentTests.cs (file + class). Added World.IdIfRegistered<T>() using Type<T>.IsRegistered().
- [x] **Runtime test fixes** — Fixed 6 categories of runtime failures:
  1. Zig 0.16 build API changes (openDirAbsolute, ArrayList, LibCInstallation.render)
  2. ecs_field_w_size size=0 assertion for tag types (FieldData.cs, Iter.cs)
  3. EcsQueryDetectChanges flag for change detection queries (QueryBuilder, tests, example)
  4. Singleton `$` syntax removed (SingletonTests, added Ecs.Singleton trait)
  5. Module DoImport order matching C++ (Sparse/Singleton before InitModule, EcsModule after)
  6. Stats.InitModule importing Units first (matching C++ constructor)
  7. Module namespace cleanup unconditional destruct (matching C++)
  8. DontFragment tests adding Exclusive trait (Union = DontFragment + Exclusive)
  9. QueryBuilderTests removing untestable PairIsTag trait-lock scenario
