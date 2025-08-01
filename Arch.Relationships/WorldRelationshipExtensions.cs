using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Core.Extensions.Dangerous;
using Arch.Core.Utils;

[assembly:InternalsVisibleTo("Arch.Relationships.Tests")]
namespace Arch.Relationships;

/// <summary>
///     The <see cref="WorldRelationshipExtensions"/> class
///     stores several extension methods for relationships handling. 
/// </summary>
public static class WorldRelationshipExtensions
{
    
#if EVENTS
    
    /// <summary>
    ///     Subscribes to entity destruction events to cleanup their relations. 
    /// </summary>
    public static void HandleRelationshipCleanup(this World world)
    {
        world.SubscribeEntityDestroyed((in Entity entity) => CleanupRelationships(world, in entity));
    }

    // TODO: Probably someone will kill me for the dark magic that happens down below. 
    /// <summary>
    ///     Cleans up all relations of the passed <see cref="Entity"/>.
    /// </summary>
    /// <param name="entity"></param>
    public static void CleanupRelationships(this World world, in Entity entity)
    {
        ref var relationships = ref world.TryGetRefRelationships<InRelationship>(entity, out var exists);
        if (!exists)
        {
            return;
        }
        
        foreach (var (target, inRelationship) in relationships.Elements)
        {
            var id = inRelationship.ComponentTypeId;
            var componentType = new ComponentType(id, 0);
            
            // Get slots, chunk and array to prevent entity.Get(type) object allocation
            ref readonly var chunk = ref world.GetChunk(target);
            var array = chunk.GetArray(componentType);
            var relationshipsArray = Unsafe.As<IRelationship[]>(array);

            var slot = world.GetSlot(target);
            var relationship = relationshipsArray[slot.Index];
            relationship.Remove(entity);
            
            if (relationship.Count == 0)
            {
                relationship.Destroy(world, target);
            }

            ref var targetRelationships = ref world.TryGetRefRelationships<InRelationship>(target, out exists);
            if (!exists)
            {
                continue;
            }

            targetRelationships.Remove(entity);
        }
    }
#endif
    

    /// <summary>
    ///     Adds a new relationship to the <see cref="Entity"/>.
    /// </summary>
    /// <param name="source">The source <see cref="Entity"/> of the relationship.</param>
    /// <param name="target">The target <see cref="Entity"/> of the relationship.</param>
    /// <typeparam name="T">The relationship type.</typeparam>
    /// <param name="relationship">The relationship instance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddRelationship<T>(this World world, Entity source, Entity target, in T relationship = default)
    {
        ref var buffer = ref world.AddOrGetRelationships<T>(source);
        buffer.Add(in relationship, target);

        var targetComp = new InRelationship(Component<Relationship<T>>.ComponentType);
        ref var targetBuffer = ref world.AddOrGetRelationships<InRelationship>(target);
        targetBuffer.Add(in targetComp, source);
    }
    

    /// <summary>
    ///     Ensures the existence of a relationship on an <see cref="Entity"/>.
    /// </summary>
    /// <typeparam name="T">The relationship type.</typeparam>
    /// <param name="source">The source <see cref="Entity"/> of the relationship.</param>
    /// <param name="target">The target <see cref="Entity"/> of the relationship.</param>
    /// <param name="relationship">The relationship value used if its being added.</param>
    /// <returns>The relationship.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AddOrGetRelationship<T>(this World world, Entity source, Entity target, in T relationship = default)
    {
        ref var relationships = ref world.TryGetRefRelationships<T>(source, out var exists);
        if (exists)
        {
            return relationships.Get(target);
        }

        world.AddRelationship(source, target, in relationship);
        return world.GetRelationship<T>(source, target);
    }
    
    /// <summary>
    ///     Ensures the existence of a buffer of relationships on an <see cref="Entity"/>.
    /// </summary>
    /// <param name="source">The source <see cref="Entity"/> of the relationships.</param>
    /// <typeparam name="T">The relationship type.</typeparam>
    /// <returns>The relationships.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ref Relationship<T> AddOrGetRelationships<T>(this World world, Entity source)
    {
        ref var component = ref world.TryGetRef<Relationship<T>>(source, out var exists);
        if (exists)
        {
            return ref component;
        }

        world.Add(source, new Relationship<T>());
        return ref world.Get<Relationship<T>>(source);
    }
    
    /// <summary>
    ///     Sets the existing relationship data.
    /// </summary>
    /// <typeparam name="T">The relationship type.</typeparam>
    /// <param name="source">The source <see cref="Entity"/> of the relationship.</param>
    /// <param name="target">The target <see cref="Entity"/> of the relationship.</param>
    /// <param name="relationship">The new data.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetRelationship<T>(this World world, Entity source, Entity target, in T relationship = default)
    {
        ref var relationships = ref world.GetRelationships<T>(source);
        relationships.Set(target, relationship);
    }

    /// <summary>
    ///     Checks if an <see cref="Entity"/> has a certain relationship.
    /// </summary>
    /// <typeparam name="T">The relationship type.</typeparam>
    /// <param name="source">The source <see cref="Entity"/> of the relationship.</param>
    /// <param name="target">The target <see cref="Entity"/> of the relationship.</param>
    /// <returns>True if it has the desired relationship, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    public static bool HasRelationship<T>(this World world, Entity source, Entity target)
    {
        ref var relationships = ref world.TryGetRefRelationships<T>(source, out var exists);
        if (!exists)
        {
            return false;
        }

        return relationships.Contains(target);
    }
    
    /// <summary>
    ///     Checks if an <see cref="Entity"/> has a certain relationship.
    /// </summary>
    /// <typeparam name="T">The relationship type.</typeparam>
    /// <param name="source">The source <see cref="Entity"/> of the relationship.</param>
    /// <returns>True if it has the desired relationship, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    public static bool HasRelationship<T>(this World world, Entity source)
    {
        return world.Has<Relationship<T>>(source);
    }

    /// <summary>
    ///     Returns a relationship of an <see cref="Entity"/>.
    /// </summary>
    /// <typeparam name="T">The relationship type.</typeparam>
    /// <param name="source">The source <see cref="Entity"/> of the relationship.</param>
    /// <param name="target">The target <see cref="Entity"/> of the relationship.</param>
    /// <returns>The relationship.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    public static T GetRelationship<T>(this World world, Entity source, Entity target)
    {
        ref var relationships = ref world.GetRelationships<T>(source);
        return relationships.Get(target);
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
    public static bool TryGetRelationship<T>(this World world, Entity source, Entity target, out T relationship)
    {
        ref var relationships = ref world.TryGetRefRelationships<T>(source, out var exists);
        if (!exists)
        {
            relationship = default;
            return false;
        }

        return relationships.TryGetValue(target, out relationship);
    }
    /// <summary>
    ///     Returns all relationships of the given type of an <see cref="Entity"/>.
    /// </summary>
    /// <typeparam name="T">The relationship type.</typeparam>
    /// <param name="source">The source <see cref="Entity"/> of the relationship.</param>
    /// <returns>A reference to the relationships.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    public static ref Relationship<T> GetRelationships<T>(this World world, Entity source)
    {
        return ref world.Get<Relationship<T>>(source);
    }

    /// <summary>
    ///     Tries to return an <see cref="Entity"/>s relationships of the specified type.
    /// </summary>
    /// <typeparam name="T">The relationship type.</typeparam>
    /// <param name="source">The <see cref="Entity"/>.</param>
    /// <param name="relationships">The found relationships.</param>
    /// <returns>True if it exists, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    internal static bool TryGetRelationships<T>(this World world, Entity source, out Relationship<T> relationships)
    {
        return world.TryGet(source, out relationships);
    }

        /// <summary>
    ///     Tries to return a reference to an <see cref="Entity"/>s relationships of the
    ///     specified type.
    /// </summary>
    /// <typeparam name="T">The relationship type.</typeparam>
    /// <param name="source">The <see cref="Entity"/>.</param>
    /// <param name="exists">True if it exists, otherwise false.</param>
    /// <returns>A reference to the relationships.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    internal static ref Relationship<T> TryGetRefRelationships<T>(this World world, Entity source, out bool exists)
    {
        return ref world.TryGetRef<Relationship<T>>(source, out exists);
    }

    /// <summary>
    ///     Removes a relationship from an <see cref="Entity"/>.
    /// </summary>
    /// <typeparam name="T">The relationship type.</typeparam>
    /// <param name="source">The <see cref="Entity"/> to remove the relationship from.</param>
    /// <param name="target">The target <see cref="Entity"/> of the relationship.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RemoveRelationship<T>(this World world, Entity source, Entity target)
    {
        ref var buffer = ref world.GetRelationships<T>(source);
        buffer.Remove(target);

        if (buffer.Count == 0)
        {
            world.Remove<Relationship<T>>(source);
        }

        ref var targetBuffer = ref world.GetRelationships<InRelationship>(target);
        targetBuffer.Remove(source);

        if (targetBuffer.Count == 0)
        {
            world.Remove<Relationship<InRelationship>>(target);
        }
    }
}
