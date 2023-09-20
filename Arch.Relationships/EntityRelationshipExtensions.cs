using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Arch.Core;

namespace Arch.Relationships;

#if !PURE_ECS

/// <summary>
///     The <see cref="EntityRelationshipExtensions"/> class
///     stores several methods to forward relationship methods from the <see cref="World"/> to the <see cref="Entity"/>.
/// </summary>
public static class EntityRelationshipExtensions
{

    /// <summary>
    ///     Adds a new relationship to the <see cref="Entity"/>.
    /// </summary>
    /// <param name="source">The source <see cref="Entity"/> of the relationship.</param>
    /// <param name="target">The target <see cref="Entity"/> of the relationship.</param>
    /// <typeparam name="T">The relationship type.</typeparam>
    /// <param name="relationship">The relationship instance.</param>
    public static void AddRelationship<T>(this in Entity source, Entity target, T relationship = default)
    {
        var world = World.Worlds[source.WorldId];
        world.AddRelationship(source, target, relationship);
    }
    
    /// <summary>
    ///     Sets a relationship to the <see cref="Entity"/> by updating its relationship data.
    /// </summary>
    /// <param name="source">The source <see cref="Entity"/> of the relationship.</param>
    /// <param name="target">The target <see cref="Entity"/> of the relationship.</param>
    /// <typeparam name="T">The relationship type.</typeparam>
    /// <param name="relationship">The relationship instance.</param>
    public static void SetRelationship<T>(this in Entity source, Entity target, T relationship = default)
    {
        var world = World.Worlds[source.WorldId];
        world.SetRelationship(source, target, relationship);
    }

    /// <summary>
    ///     Checks if an <see cref="Entity"/> has a certain relationship.
    /// </summary>
    /// <typeparam name="T">The relationship type.</typeparam>
    /// <param name="source">The source <see cref="Entity"/> of the relationship.</param>
    /// <param name="target">The target <see cref="Entity"/> of the relationship.</param>
    /// <returns>True if it has the desired relationship, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    public static bool HasRelationship<T>(this in Entity source, Entity target)
    {
        var world = World.Worlds[source.WorldId];
        return world.HasRelationship<T>(source, target);
    }
    
    /// <summary>
    ///     Checks if an <see cref="Entity"/> has a certain relationship.
    /// </summary>
    /// <typeparam name="T">The relationship type.</typeparam>
    /// <param name="source">The source <see cref="Entity"/> of the relationship.</param>
    /// <returns>True if it has the desired relationship, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    public static bool HasRelationship<T>(this in Entity source)
    {
        var world = World.Worlds[source.WorldId];
        return world.HasRelationship<T>(source);
    }
    
    /// <summary>
    ///     Returns a relationship of an <see cref="Entity"/>.
    /// </summary>
    /// <typeparam name="T">The relationship type.</typeparam>
    /// <param name="source">The source <see cref="Entity"/> of the relationship.</param>
    /// <param name="target">The target <see cref="Entity"/> of the relationship.</param>
    /// <returns>The relationship.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    public static T GetRelationship<T>(this in Entity source, Entity target)
    {
        var world = World.Worlds[source.WorldId];
        return world.GetRelationship<T>(source, target);
    }
    
    /// <summary>
    ///     Returns a relationship of an <see cref="Entity"/>.
    /// </summary>
    /// <typeparam name="T">The relationship type.</typeparam>
    /// <param name="source">The source <see cref="Entity"/> of the relationship.</param>
    /// <returns>The <see cref="Relationship{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    public static ref Relationship<T> GetRelationships<T>(this in Entity source)
    {
        var world = World.Worlds[source.WorldId];
        return ref world.GetRelationships<T>(source);
    }
    
    /// <summary>
    ///     Tries to return an <see cref="Entity"/>s relationship of the specified type.
    ///     Will copy the relationship if its a struct.
    /// </summary>
    /// <typeparam name="T">The relationship type.</typeparam>
    /// <param name="source">The source <see cref="Entity"/> of the relationship.</param>
    /// <param name="target">The target <see cref="Entity"/> of the relationship.</param>
    /// <param name="relationship">The found relationship.</param>
    /// <returns>True if it exists, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    public static bool TryGetRelationship<T>(this in Entity source, Entity target, out T relationship)
    {
        var world = World.Worlds[source.WorldId];
        return world.TryGetRelationship(source, target, out relationship);
    }
    
    /// <summary>
    ///     Removes a relationship from an <see cref="Entity"/>.
    /// </summary>
    /// <typeparam name="T">The relationship type.</typeparam>
    /// <param name="source">The <see cref="Entity"/> to remove the relationship from.</param>
    /// <param name="target">The target <see cref="Entity"/> of the relationship.</param>
    public static void RemoveRelationship<T>(this in Entity source, Entity target)
    {
        var world = World.Worlds[source.WorldId];
        world.RemoveRelationship<T>(source, target);
    }
}

#endif
