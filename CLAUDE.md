# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Flecs.NET is a high-level C# wrapper for [flecs](https://github.com/SanderMertens/flecs), an Entity Component System (ECS) framework. It targets .NET 9 and uses unsafe/pointer-heavy code to interop with the native C library. The project supports Windows, macOS, Linux, iOS, WASM, and Android.

## Build Commands

```bash
# Restore dependencies
dotnet restore

# Build everything (compiles C# and cross-compiles native libs via Zig)
dotnet build

# Run tests
dotnet test

# Run a specific test class
dotnet test --filter "FullyQualifiedName~Flecs.NET.Tests.CSharp.Core.EntityTests"

# Run a single test
dotnet test --filter "FullyQualifiedName~Flecs.NET.Tests.CSharp.Core.EntityTests.MethodName"

# Run an example (underscore-separated path relative to Examples project)
dotnet run --project src/Flecs.NET.Examples --property:Example=Entities_Basics

# Regenerate C# bindings from flecs C headers
dotnet run --project src/Flecs.NET.Bindgen

# Regenerate code-generated files
dotnet run --project src/Flecs.NET.Codegen
```

## Architecture

### Project Dependency Chain

```
Flecs.NET (high-level wrapper)
  -> Flecs.NET.Bindings (auto-generated P/Invoke bindings)
    -> Flecs.NET.Native (Zig-compiled native flecs library)
```

### Key Projects

- **`src/Flecs.NET/`** - The main wrapper library. Struct-based API mirroring the C++ flecs wrapper.
- **`src/Flecs.NET.Bindings/`** - Auto-generated raw C bindings (`Flecs.g.cs`, `FlecsExtensions.g.cs`). Regenerate with `Flecs.NET.Bindgen`.
- **`src/Flecs.NET.Native/`** - Builds the native flecs C library using Zig cross-compilation. The `.csproj` maps .NET RIDs to Zig targets.
- **`src/Flecs.NET.Codegen/`** - Code generator that produces generic type variants (up to 16 type parameters) for queries, iterables, invokers, etc. Output goes to `src/Flecs.NET/Generated/`.
- **`src/Flecs.NET.Tests/`** - xUnit tests. `Cpp/` contains ports of the C++ flecs test suite; `CSharp/` contains C#-specific tests.
- **`src/Flecs.NET.Examples/`** - Example programs selectable via the `Example` MSBuild property.
- **`native/flecs/`** - Git submodule containing the upstream flecs C source.

### Code Generation Pattern

The codegen (`Flecs.NET.Codegen`) is central to the project. Many core types are `partial struct` with hand-written base logic in `src/Flecs.NET/Core/` and generated generic overloads in `src/Flecs.NET/Generated/<TypeName>/`. The generator produces variants for 1-16 generic type parameters (e.g., `Query<T0>` through `Query<T0,...,T15>`). After modifying any generator in `src/Flecs.NET.Codegen/Generators/`, you must re-run the codegen.

### Core Type Structure

All core types are **readonly/mutable structs wrapping native pointers** (not classes). Key types:
- `World` - Container for all ECS data, wraps `ecs_world_t*`
- `Entity` - Wraps an `Id` (which contains `ecs_world_t*` + entity id)
- `Component<T>` - Registers and wraps a component type
- `Query` / `Query<T0,...>` - Wraps `ecs_query_t*`, generic variants are code-generated
- `System_` / `Observer` - ECS systems and observers with builder pattern
- `Iter` - Iterator passed to system/query callbacks

### BindingContext System

`src/Flecs.NET/Core/BindingContext/` manages the bridge between managed C# delegates and native C function pointers. It handles preventing GC collection of delegates passed to native code and routing callbacks back to managed code.

### Type Hook Interfaces

`src/Flecs.NET/Core/Hooks/` defines interfaces (`ICtorHook`, `IDtorHook`, `ICopyHook`, `IMoveHook`, `IOnAddHook`, `IOnRemoveHook`, `IOnSetHook`) that component types can implement for lifecycle callbacks.

### Packaging

The project produces separate Debug and Release NuGet packages (e.g., `Flecs.NET.Debug`, `Flecs.NET.Release`). The `Flecs.NET` meta-package selects between them based on the consumer's `$(Optimize)` property.

## Code Conventions

- All wrapper types use `static Flecs.NET.Bindings.flecs` for direct access to native functions
- Components are typically `record struct` types
- The `Ecs` static class (`src/Flecs.NET/Core/Ecs/`) contains delegates, constants, configuration, and utility methods
- Builder pattern is used extensively (e.g., `QueryBuilder`, `ObserverBuilder`, `SystemBuilder`)
- Native interop uses raw pointers throughout (`unsafe` context)
