namespace Flecs.NET.Core.Hooks;

/// <summary>
///     Interface for registering an on replace hook callback with the provided type.
///     OnReplace fires when a component value is replaced (not on initial set).
///     The callback receives the OLD value before the replacement occurs,
///     unlike <see cref="IOnSetHook{T}"/> which fires after the value is set.
/// </summary>
/// <typeparam name="T">The component type.</typeparam>
public interface IOnReplaceHook<T>
{
    /// <summary>
    ///     The on replace hook callback. Called before the component data is replaced.
    ///     The <paramref name="data"/> parameter contains the old value.
    /// </summary>
    /// <param name="it">The iterator.</param>
    /// <param name="i">The entity row.</param>
    /// <param name="data">The reference to the component (old value, before replacement).</param>
    public static abstract void OnReplace(Iter it, int i, ref T data);

    internal static unsafe class FunctionPointer<TInterface> where TInterface : IOnReplaceHook<T>
    {
        public static delegate*<Iter, int, ref T, void> Get()
        {
            return &TInterface.OnReplace;
        }
    }
}
