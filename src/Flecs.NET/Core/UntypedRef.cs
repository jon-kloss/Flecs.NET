using System;
using static Flecs.NET.Bindings.flecs;

namespace Flecs.NET.Core;

/// <summary>
///     Untyped reference to a component from a specific entity.
/// </summary>
public unsafe struct UntypedRef : IEquatable<UntypedRef>
{
    private ecs_world_t* _world;
    private ecs_ref_t _ref;

    /// <summary>
    ///     A reference to the world.
    /// </summary>
    public ref ecs_world_t* World => ref _world;

    /// <summary>
    ///     Creates an untyped ref.
    /// </summary>
    /// <param name="world"></param>
    /// <param name="entity"></param>
    /// <param name="id"></param>
    public UntypedRef(ecs_world_t* world, ulong entity, ulong id)
    {
        _world = world == null ? null : ecs_get_world(world);
        _ref = ecs_ref_init_id(world, entity, id);
    }

    /// <summary>
    ///     Gets a pointer to the ref component.
    /// </summary>
    /// <returns></returns>
    public void* GetPtr()
    {
        fixed (ecs_ref_t* refPtr = &_ref)
        {
            return ecs_ref_get_id(World, refPtr, _ref.id);
        }
    }

    /// <summary>
    ///     Attempts to get a pointer to the ref component.
    /// </summary>
    /// <returns></returns>
    public void* TryGetPtr()
    {
        if (World == null || _ref.entity == 0)
            return null;

        return GetPtr();
    }

    /// <summary>
    ///     Returns whether the reference is valid.
    /// </summary>
    /// <returns></returns>
    public bool Has()
    {
        return TryGetPtr() != null;
    }

    /// <summary>
    ///     Returns the entity associated with the ref.
    /// </summary>
    /// <returns></returns>
    public Entity Entity()
    {
        return new Entity(World, _ref.entity);
    }

    /// <summary>
    ///     Returns the component id associated with the ref.
    /// </summary>
    /// <returns></returns>
    public Id Component()
    {
        return new Id(World, _ref.id);
    }

    /// <summary>
    ///     Returns whether the reference is valid.
    /// </summary>
    /// <param name="reference">The ref object.</param>
    /// <returns></returns>
    public static bool ToBoolean(UntypedRef reference)
    {
        return reference.Has();
    }

    /// <summary>
    ///     Returns whether the reference is valid.
    /// </summary>
    /// <param name="reference">The ref object.</param>
    /// <returns></returns>
    public static implicit operator bool(UntypedRef reference)
    {
        return ToBoolean(reference);
    }

    /// <summary>
    ///     Checks if two <see cref="UntypedRef"/> instances are equal.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(UntypedRef other)
    {
        return Equals(_ref, other._ref);
    }

    /// <summary>
    ///     Checks if two <see cref="UntypedRef"/> instances are equal.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object? obj)
    {
        return obj is UntypedRef other && Equals(other);
    }

    /// <summary>
    ///     Returns the hash code of the <see cref="UntypedRef"/>.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return _ref.GetHashCode();
    }

    /// <summary>
    ///     Checks if two <see cref="UntypedRef"/> instances are equal.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(UntypedRef left, UntypedRef right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Checks if two <see cref="UntypedRef"/> instances are not equal.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(UntypedRef left, UntypedRef right)
    {
        return !(left == right);
    }
}
