using Arch.Core;
using Arch.Core.Extensions;
using Arch.Core.Extensions.Dangerous;
using Arch.Core.Utils;
using Arch.LowLevel.Jagged;
using MessagePack;
using MessagePack.Formatters;
using System.Runtime.CompilerServices;
using Utf8Json;

namespace Arch.Persistence;


/// <summary>
///     The <see cref="SingleEntityFormatter"/> class
///     is a <see cref="IJsonFormatter"/> to (de)serialize a single <see cref="Entity"/>to or from json.
/// </summary>
public partial class SingleEntityFormatter : IMessagePackFormatter<Entity>
{

    public void Serialize(ref MessagePackWriter writer, Entity value, MessagePackSerializerOptions options)
    {
        // Write id
        writer.WriteInt32(value.Id);

#if !PURE_ECS

        // Write world
        writer.WriteInt32(value.WorldId);
#endif

        // Write size
        var componentTypes = value.GetComponentTypes();
        writer.WriteInt32(componentTypes.Length);

        // Write components
        foreach (ref var type in componentTypes.AsSpan())
        {
            // Write type
            MessagePackSerializer.Serialize(ref writer, type, options);

            // Write component
            var cmp = value.Get(type);
            MessagePackSerializer.Serialize(ref writer, cmp, options);
        }
    }

    public Entity Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        // Read id
        var entityId = reader.ReadInt32();

#if !PURE_ECS

        // Read world id
        var worldId = reader.ReadInt32();
#endif

        // Read size
        var size = reader.ReadInt32();
        var components = new object[size];

        // Read components
        for (var index = 0; index < size; index++)
        {
            // Read type
            var type = MessagePackSerializer.Deserialize<ComponentType>(ref reader, options);
            var cmp = MessagePackSerializer.Deserialize(type, ref reader, options);
            components[index] = cmp;
        }

        // Create the entity
        var entity = EntityWorld.Create();
        EntityWorld.AddRange(entity, components.AsSpan());
        return entity;
    }
}

/// <summary>
///     The <see cref="EntityFormatter"/> class
///     is a formatter that (de)serializes <see cref="Entity"/> structs. 
/// </summary>
public partial class EntityFormatter : IMessagePackFormatter<Entity>
{
    public void Serialize(ref MessagePackWriter writer, Entity value, MessagePackSerializerOptions options)
    {
        writer.WriteInt32(value.Id);
    }

    public Entity Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        // Read id
        var id = reader.ReadInt32();
        return DangerousEntityExtensions.CreateEntityStruct(id, WorldId);
    }
}

/// <summary>
///     The <see cref="ArrayFormatter"/> class
///     is a <see cref="IJsonFormatter{Array}"/> to (de)serialize <see cref="Array"/>s to or from json.
/// </summary>
public partial class ArrayFormatter : IMessagePackFormatter<Array>
{
    public void Serialize(ref MessagePackWriter writer, Array value, MessagePackSerializerOptions options)
    {
        var type = value.GetType().GetElementType();

        // Write type and size
        MessagePackSerializer.Serialize(ref writer, type, options);
        writer.WriteUInt32((uint)value.Length);

        // Write array
        for (var index = 0; index < value.Length; index++)
        {
            var obj = value.GetValue(index);
            MessagePackSerializer.Serialize(ref writer, obj, options);
        }
    }

    public Array Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        // Write type and size
        var type = MessagePackSerializer.Deserialize<Type>(ref reader, options);
        var size = reader.ReadUInt32();

        // Create array
        var array = Array.CreateInstance(type, size);

        // Read array
        for (var index = 0; index < size; index++)
        {
            var obj = MessagePackSerializer.Deserialize(type, ref reader, options);
            array.SetValue(obj, index);
        }
        return array;
    }
}

/// <summary>
///     The <see cref="JaggedArrayFormatter{T}"/> class
///     (de)serializes a <see cref="JaggedArray{T}"/>.
/// </summary>
/// <typeparam name="T">The type stored in the <see cref="JaggedArray{T}"/>.</typeparam>
public partial class JaggedArrayFormatter<T> : IMessagePackFormatter<JaggedArray<T>>
{
    private const int CpuL1CacheSize = 16_000;
    private readonly T _filler;

    public JaggedArrayFormatter(T filler)
    {
        _filler = filler;
    }

    public void Serialize(ref MessagePackWriter writer, JaggedArray<T> value, MessagePackSerializerOptions options)
    {

        // Write length/capacity and items
        writer.WriteInt32(value.Capacity);
        for (var index = 0; index < value.Capacity; index++)
        {
            var item = value[index];
            MessagePackSerializer.Serialize(ref writer, item, options);
        }
    }

    public JaggedArray<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var capacity = reader.ReadInt32();
        var jaggedArray = new JaggedArray<T>(CpuL1CacheSize / Unsafe.SizeOf<T>(), _filler,capacity);

        for (var index = 0; index < capacity; index++)
        {
            var item = MessagePackSerializer.Deserialize<T>(ref reader, options);
            jaggedArray.Add(index, item);
        }

        return jaggedArray;
    }
}

/// <summary>
///     The <see cref="ComponentTypeFormatter"/> class
///     is a <see cref="IJsonFormatter{ComponentType}"/> to (de)serialize <see cref="ComponentType"/>s to or from json.
/// </summary>
public partial class ComponentTypeFormatter : IMessagePackFormatter<ComponentType>
{
    public void Serialize(ref MessagePackWriter writer, ComponentType value, MessagePackSerializerOptions options)
    {
        // Write id
        writer.WriteUInt32((uint)value.Id);

        // Write bytesize
        writer.WriteUInt32((uint)value.ByteSize);
    }

    public ComponentType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var id = reader.ReadUInt32();
        var bytesize = reader.ReadUInt32();

        return new ComponentType((int)id, (int)bytesize);
    }
}

/// <summary>
///     The <see cref="ComponentTypeFormatter"/> class
///     is a <see cref="IJsonFormatter{ComponentType}"/> to (de)serialize <see cref="ComponentType"/>s to or from json.
/// </summary>
public partial class EntitySlotFormatter : IMessagePackFormatter<(Archetype, (int,int))>
{
    public void Serialize(ref MessagePackWriter writer, (Archetype, (int, int)) value, MessagePackSerializerOptions options)
    {
        // Write chunk index
        writer.WriteUInt32((uint)value.Item2.Item1);

        // Write entity index
        writer.WriteUInt32((uint)value.Item2.Item2);
    }

    public (Archetype, (int, int)) Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        
        // Read chunk index and entity index
        var chunkIndex = reader.ReadUInt32();
        var entityIndex = reader.ReadUInt32();

        return (null, ((int)chunkIndex, (int)entityIndex));
    }
}


/// <summary>
///     The <see cref="WorldFormatter"/> class
///     is a <see cref="IJsonFormatter{World}"/> to (de)serialize <see cref="World"/>s to or from json.
/// </summary>
public partial class WorldFormatter : IMessagePackFormatter<World>
{
    public void Serialize(ref MessagePackWriter writer, World value, MessagePackSerializerOptions options)
    {
        // Write entity info
        MessagePackSerializer.Serialize(ref writer, value.GetVersions(), options);

        // Write slots
        MessagePackSerializer.Serialize(ref writer, value.GetSlots(), options);

        //Write recycled entity ids
        var recycledEntityIDs = value.GetRecycledEntityIds();
        MessagePackSerializer.Serialize(ref writer, recycledEntityIDs, options);

        // Write archetypes
        writer.WriteUInt32((uint)value.Archetypes.Count);
        foreach (var archetype in value)
        {
            MessagePackSerializer.Serialize(ref writer, archetype, options);
        }
    }

    public World Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        // Create world and setup formatter
        var world = World.Create();
        var archetypeFormatter = options.Resolver.GetFormatter<Archetype>() as ArchetypeFormatter;
        var entityFormatter = options.Resolver.GetFormatter<Entity>() as EntityFormatter;
        entityFormatter.WorldId = world.Id;
        archetypeFormatter.World = world;

        // Read versions
        var versions = MessagePackSerializer.Deserialize<JaggedArray<int>>(ref reader, options);

        // Read slots
        var slots = MessagePackSerializer.Deserialize<JaggedArray<(Archetype, (int, int))>>(ref reader, options);

        //Read recycled entity ids
        var recycledEntityIDs = MessagePackSerializer.Deserialize<List<(int, int)>>(ref reader, options);

        // Forward values to the world
        world.SetVersions(versions);
        world.SetRecycledEntityIds(recycledEntityIDs);
        world.SetSlots(slots);
        world.EnsureCapacity(versions.Capacity);
        
        // Read archetypes
        var size = reader.ReadInt32();
        List<Archetype> archetypes = new();

        for (var index = 0; index < size; index++)
        {
            var archetype = archetypeFormatter.Deserialize(ref reader, options);
            archetypes.Add(archetype);
        }
        
        // Set archetypes
        world.SetArchetypes(archetypes);
        return world;
    }
}


/// <summary>
///     The <see cref="ArchetypeFormatter"/> class
///     is a <see cref="IJsonFormatter{Archetype}"/> to (de)serialize <see cref="Archetype"/>s to or from json.
/// </summary>
public partial class ArchetypeFormatter : IMessagePackFormatter<Archetype>
{

    public void Serialize(ref MessagePackWriter writer, Archetype value, MessagePackSerializerOptions options)
    {
        // Setup formatters
        var types = value.Types;
        var chunks = value.Chunks;
        var chunkFormatter = options.Resolver.GetFormatter<Chunk>() as ChunkFormatter;
        chunkFormatter.Types = types;

        // Write type array
        MessagePackSerializer.Serialize(ref writer, types, options);

        // Write lookup array
        MessagePackSerializer.Serialize(ref writer, value.GetLookupArray(), options);

        // Write chunk size
        writer.WriteUInt32((uint)value.ChunkCount);

        // Write chunks 
        for (var index = 0; index < value.ChunkCount; index++)
        {
            ref var chunk = ref chunks[index];
            chunkFormatter.Serialize(ref writer, chunk, options);
        }
    }

    public Archetype Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {

        var chunkFormatter = options.Resolver.GetFormatter<Chunk>() as ChunkFormatter;

        // Types
        var types = MessagePackSerializer.Deserialize<ComponentType[]>(ref reader, options);

        // Archetype lookup array
        var lookupArray = MessagePackSerializer.Deserialize<int[]>(ref reader, options);

        // Archetype chunk size and list
        var chunkSize = reader.ReadUInt32();

        // Create archetype
        var chunks = new List<Chunk>((int)chunkSize);
        var archetype = DangerousArchetypeExtensions.CreateArchetype(types.ToArray());
        archetype.SetSize((int)chunkSize);

        // Pass types and lookup array to the chunk formatter for saving performance and memory
        chunkFormatter.World = World;
        chunkFormatter.Archetype = archetype;
        chunkFormatter.Types = types;
        chunkFormatter.LookupArray = lookupArray;

        // Deserialise each chunk and put it into the archetype. 
        var entities = 0;
        for (var index = 0; index < chunkSize; index++)
        {
            var chunk = chunkFormatter.Deserialize(ref reader, options);
            chunks.Add(chunk);
            entities += chunk.Size;
        }

        archetype.SetChunks(chunks);
        archetype.SetEntities(entities);
        return archetype;
    }
}

/// <summary>
///     The <see cref="ChunkFormatter"/> class
///     is a <see cref="IJsonFormatter{Chunk}"/> to (de)serialize <see cref="Chunk"/>s to or from json.
/// </summary>
public partial class ChunkFormatter : IMessagePackFormatter<Chunk>
{
    public void Serialize(ref MessagePackWriter writer, Chunk value, MessagePackSerializerOptions options)
    {
        // Write size
        writer.WriteUInt32((uint)value.Size);

        // Write capacity
        writer.WriteUInt32((uint)value.Capacity);

        // Write entitys
        MessagePackSerializer.Serialize(ref writer, value.Entities, options);

        // Persist arrays as an array...
        for (var index = 0; index < Types.Length; index++)
        {
            ref var type = ref Types[index];

            // Write array itself
            var array = value.GetArray(type);
            MessagePackSerializer.Serialize(ref writer, array, options);
        }
    }

    public Chunk Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        // Read chunk size
        var size = reader.ReadUInt32();

        // Read chunk size
        var capacity = reader.ReadUInt32();

        // Read entities
        var entities = MessagePackSerializer.Deserialize<Entity[]>(ref reader, options);

        // Create chunk
        var chunk = DangerousChunkExtensions.CreateChunk((int)capacity, LookupArray, Types);
        entities.CopyTo(chunk.Entities, 0);
        chunk.SetSize((int)size);

        // Updating World.EntityInfoStorage to their new archetype
        for (var index = 0; index < size; index++)
        {
            ref var entity = ref chunk.Entity(index);
            entity = DangerousEntityExtensions.CreateEntityStruct(entity.Id, World.Id);
            World.SetArchetype(entity, Archetype);
        }

        // Persist arrays as an array...
        for (var index = 0; index < Types.Length; index++)
        {
            // Read array of the type
            var array = MessagePackSerializer.Deserialize<Array>(ref reader, options);
            var chunkArray = chunk.GetArray(array.GetType().GetElementType());
            Array.Copy(array, chunkArray, (int)size);
        }

        return chunk;
    }
}


