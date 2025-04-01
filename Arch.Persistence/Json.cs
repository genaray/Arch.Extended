using Arch.Core;
using Arch.Core.Extensions;
using Arch.Core.Extensions.Dangerous;
using Arch.Core.Utils;
using Arch.LowLevel.Jagged;
using CommunityToolkit.HighPerformance;
using System.Runtime.CompilerServices;
using Utf8Json;

namespace Arch.Persistence;


/// <summary>
///     The <see cref="SingleEntityFormatter"/> class
///     is a <see cref="IJsonFormatter{Entity}"/> to (de)serialize a single <see cref="Entity"/>to or from json.
/// </summary>
public partial class SingleEntityFormatter : IJsonFormatter<Entity>
{

    /// <summary>
    ///     The <see cref="EntityWorld"/> the entity belongs to. 
    /// </summary>
    internal World EntityWorld { get; set; }

    public void Serialize(ref JsonWriter writer, Entity value, IJsonFormatterResolver formatterResolver)
    {
        writer.WriteBeginObject();

        // Write id
        writer.WritePropertyName("id");
        writer.WriteInt32(value.Id);
        writer.WriteValueSeparator();

#if !PURE_ECS

        // Write world
        writer.WritePropertyName("worldId");
        writer.WriteInt32(value.WorldId);
        writer.WriteValueSeparator();

#endif

        // Write size
        var componentTypes = value.GetComponentTypes();
        writer.WritePropertyName("size");
        writer.WriteInt32(componentTypes.Length);
        writer.WriteValueSeparator();

        // Write components
        writer.WritePropertyName("components");
        writer.WriteBeginArray();
        foreach (ref var type in componentTypes.AsSpan())
        {
            // Write type
            writer.WriteBeginObject();
            writer.WritePropertyName("type");
            JsonSerializer.Serialize(ref writer, type);
            writer.WriteValueSeparator();

            // Write component
            writer.WritePropertyName("component");
            var cmp = value.Get(type);
            JsonSerializer.NonGeneric.Serialize(ref writer, cmp, formatterResolver);
            writer.WriteEndObject();
            writer.WriteValueSeparator();
        }
        writer.AdvanceOffset(-1);

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    public Entity Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        reader.ReadIsBeginObject();

        // Read id
        reader.ReadPropertyName();
        var entityId = reader.ReadInt32();
        reader.ReadIsValueSeparator();

#if !PURE_ECS

        // Read world id
        reader.ReadPropertyName();
        var worldId = reader.ReadInt32();
        reader.ReadIsValueSeparator();

#endif

        // Read size
        reader.ReadPropertyName();
        var size = reader.ReadInt32();
        reader.ReadIsValueSeparator();

        var components = new object[size];
        var count = 0;

        // Read components
        reader.ReadPropertyName();
        reader.ReadIsBeginArray();
        while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
        {
            reader.ReadIsBeginObject();

            // Read type
            reader.ReadPropertyName();
            var type = JsonSerializer.Deserialize<ComponentType>(ref reader);
            reader.ReadIsValueSeparator();

            reader.ReadPropertyName();
            var cmp = JsonSerializer.NonGeneric.Deserialize(type.Type, ref reader, formatterResolver);
            components[count - 1] = cmp;
            reader.ReadIsEndObject();
        }

        // Creat the entity
        var entity = EntityWorld.Create();
        EntityWorld.AddRange(entity, components.AsSpan());

        reader.ReadIsEndObject();
        return entity;
    }
}

public partial class EntityFormatter : IJsonFormatter<Entity>
{

    /// <summary>
    ///     The <see cref="World.Id"/> all deserialized <see cref="Entity"/>s will belong to.
    ///     <remarks>Due to the nature of deserialisation and changing world landscape we need to assign new WorldIds to the deserialized entities.</remarks>
    /// </summary>
    internal int WorldId { get; set; }

    public void Serialize(ref JsonWriter writer, Entity value, IJsonFormatterResolver formatterResolver)
    {
        writer.WriteInt32(value.Id);
        writer.WriteValueSeparator();
        writer.WriteInt32(value.Version);
    }

    public Entity Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        // Read id
        var id = reader.ReadInt32();
        reader.ReadIsValueSeparator();
        var version = reader.ReadInt32();
        return DangerousEntityExtensions.CreateEntityStruct(id, WorldId, version);
    }
}


/// <summary>
///     The <see cref="ArrayFormatter"/> class
///     is a <see cref="IJsonFormatter{Array}"/> to (de)serialize <see cref="Array"/>s to or from json.
/// </summary>
public partial class ArrayFormatter : IJsonFormatter<Array>
{
    public void Serialize(ref JsonWriter writer, Array value, IJsonFormatterResolver formatterResolver)
    {
        var type = value.GetType().GetElementType();

        // Write type and size
        writer.WriteBeginObject();
        writer.WritePropertyName("type");
        JsonSerializer.Serialize(ref writer, type, formatterResolver);
        writer.WriteValueSeparator();

        writer.WritePropertyName("length");
        writer.WriteUInt32((uint)value.Length);
        writer.WriteValueSeparator();

        // Write array
        writer.WritePropertyName("items");
        writer.WriteBeginArray();
        for (var index = 0; index < value.Length; index++)
        {
            var obj = value.GetValue(index);
            JsonSerializer.NonGeneric.Serialize(ref writer, obj, formatterResolver);
            writer.WriteValueSeparator();
        }
        writer.AdvanceOffset(-1);
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    public Array Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        // Write type and size
        reader.ReadIsBeginObject();
        reader.ReadPropertyName();
        var type = JsonSerializer.Deserialize<Type>(ref reader, formatterResolver);
        reader.ReadIsValueSeparator();

        reader.ReadPropertyName();
        var size = reader.ReadUInt32();
        reader.ReadIsValueSeparator();

        // Create array
        var array = Array.CreateInstance(type, size);

        // Read array
        reader.ReadPropertyName();
        reader.ReadIsBeginArray();
        for (var index = 0; index < size; index++)
        {
            var obj = JsonSerializer.NonGeneric.Deserialize(type, ref reader, formatterResolver);
            array.SetValue(obj, index);
            reader.ReadIsValueSeparator();
        }
        reader.ReadIsEndArray();
        reader.ReadIsEndObject();
        return array;
    }
}

/// <summary>
///     The <see cref="JaggedArrayFormatter{T}"/> class
///     (de)serializes a <see cref="JaggedArray{T}"/>.
/// </summary>
/// <typeparam name="T">The type stored in the <see cref="JaggedArray{T}"/>.</typeparam>
public partial class JaggedArrayFormatter<T> : IJsonFormatter<JaggedArray<T>>
{
    public void Serialize(ref JsonWriter writer, JaggedArray<T> value, IJsonFormatterResolver formatterResolver)
    {
        writer.WriteBeginObject();

        // Write length/capacity and items
        writer.WritePropertyName("capacity");
        writer.WriteInt32(value.Capacity);
        writer.WriteValueSeparator();

        // Write items
        writer.WritePropertyName("items");
        writer.WriteBeginArray();

        for (var index = 0; index < value.Capacity; index++)
        {
            var item = value[index];
            JsonSerializer.Serialize(ref writer, item, formatterResolver);
            writer.WriteValueSeparator();
        }

        // Cut last value seperator
        if (value.Capacity > 0)
        {
            writer.AdvanceOffset(-1);
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    public JaggedArray<T> Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        reader.ReadIsBeginObject();

        // Read capacity;
        reader.ReadPropertyName();
        var capacity = reader.ReadInt32();
        reader.ReadIsValueSeparator();

        // Read items
        var jaggedArray = new JaggedArray<T>(CpuL1CacheSize / Unsafe.SizeOf<T>(), _filler,capacity);
        reader.ReadPropertyName();
        reader.ReadIsBeginArray();
        for (var index = 0; index < capacity; index++)
        {
            var item = JsonSerializer.Deserialize<T>(ref reader, formatterResolver);
            jaggedArray.Add(index, item);
            reader.ReadIsValueSeparator();
        }
        reader.ReadIsEndArray();
        reader.ReadIsEndObject();

        return jaggedArray;
    }
}

/// <summary>
///     The <see cref="ComponentTypeFormatter"/> class
///     is a <see cref="IJsonFormatter{ComponentType}"/> to (de)serialize <see cref="ComponentType"/>s to or from json.
/// </summary>
public partial class ComponentTypeFormatter : IJsonFormatter<ComponentType>
{
    public void Serialize(ref JsonWriter writer, ComponentType value, IJsonFormatterResolver formatterResolver)
    {
        writer.WriteBeginObject();

        // Write id
        writer.WritePropertyName("id");
        writer.WriteUInt32((uint)value.Id);
        writer.WriteValueSeparator();

        // Write bytesize
        writer.WritePropertyName("byteSize");
        writer.WriteUInt32((uint)value.ByteSize);

        writer.WriteEndObject();
    }

    public ComponentType Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        reader.ReadIsBeginObject();

        reader.ReadPropertyName();
        var id = reader.ReadUInt32();
        reader.ReadIsValueSeparator();

        reader.ReadPropertyName();
        var bytesize = reader.ReadUInt32();
        reader.ReadIsValueSeparator();

        reader.ReadIsEndObject();

        return new ComponentType((int)id, (int)bytesize);
    }
}

/// <summary>
///     The <see cref="ComponentTypeFormatter"/> class
///     is a <see cref="IJsonFormatter{ComponentType}"/> to (de)serialize <see cref="Signature"/>s to or from json.
/// </summary>
public partial class SignatureFormatter : IJsonFormatter<Signature>
{
    public void Serialize(ref JsonWriter writer, Signature value, IJsonFormatterResolver formatterResolver)
    {
        var componentTypeFormatter = formatterResolver.GetFormatter<ComponentType>() as ComponentTypeFormatter;

        writer.WriteBeginObject();

        // write Count
        writer.WritePropertyName("count");
        writer.WriteUInt32((uint)value.Count);
        writer.WriteValueSeparator();
        
        // Write components
        writer.WritePropertyName("components");
        writer.WriteBeginArray();
        
        foreach (var type in value.Components)
        {
            componentTypeFormatter.Serialize(ref writer, type, formatterResolver);
            writer.WriteValueSeparator();
        }
        
        // Cut last value seperator
        if (value.Count > 0)
        {
            writer.AdvanceOffset(-1);
        }
        
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    public Signature Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var componentTypeFormatter = formatterResolver.GetFormatter<ComponentType>() as ComponentTypeFormatter;
        
        reader.ReadIsBeginObject();
        reader.ReadPropertyName();
        
        // Read count
        var count = (int)reader.ReadUInt32();
        reader.ReadIsValueSeparator();
        
        // Read types
        reader.ReadPropertyName();
        reader.ReadIsBeginArray();
        
        var componentTypes = new ComponentType[count];
        count = 0;
        while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
        {
            var archetype = componentTypeFormatter.Deserialize(ref reader, formatterResolver);
            componentTypes[count - 1] = (archetype);
        }

        // Set archetypes
        reader.ReadIsEndObject();
        return new Signature(componentTypes);
    }
}

/// <summary>
///     The <see cref="ComponentTypeFormatter"/> class
///     is a <see cref="IJsonFormatter{ComponentType}"/> to (de)serialize <see cref="ComponentType"/>s to or from json.
/// </summary>
public partial class EntitySlotFormatter : IJsonFormatter<EntityData>
{
    public void Serialize(ref JsonWriter writer, EntityData value, IJsonFormatterResolver options)
    {
        writer.WriteBeginObject();
        
        // Write chunk index
        writer.WritePropertyName("chunkIndex");
        writer.WriteUInt32((uint)value.Slot.ChunkIndex);
        writer.WriteValueSeparator();

        // Write entity index
        writer.WritePropertyName("index");
        writer.WriteUInt32((uint)value.Slot.Index);
        
        writer.WriteEndObject();
    }

    public EntityData Deserialize(ref JsonReader reader, IJsonFormatterResolver options)
    {
        reader.ReadIsBeginObject();

        // Read chunk index
        reader.ReadPropertyName();
        var chunkIndex = reader.ReadUInt32();
        reader.ReadIsValueSeparator();
        
        // Read entity index
        reader.ReadPropertyName();
        var entityIndex = reader.ReadUInt32();

        reader.ReadIsEndObject();
        return new EntityData(null, new Slot((int)entityIndex, (int)chunkIndex));
    }
}

/// <summary>
///     The <see cref="WorldFormatter"/> class
///     is a <see cref="IJsonFormatter{World}"/> to (de)serialize <see cref="World"/>s to or from json.
/// </summary>
public partial class WorldFormatter : IJsonFormatter<World>
{
    public void Serialize(ref JsonWriter writer, World value, IJsonFormatterResolver formatterResolver)
    {
        //var archetypeFormatter = formatterResolver.GetFormatter<Archetype>();
        //var versionsFormatter = formatterResolver.GetFormatter<int[][]>();
        //var slotFormatter = formatterResolver.GetFormatter<(int,int)[][]>();

        writer.WriteBeginObject();

        // Write meta data
        writer.WritePropertyName("baseChunkSize");
        writer.WriteUInt32((uint)value.BaseChunkSize);
        writer.WriteValueSeparator();
        
        writer.WritePropertyName("baseChunkEntityCount");
        writer.WriteUInt32((uint)value.BaseChunkEntityCount);
        writer.WriteValueSeparator();
        
        // Write slots
        writer.WritePropertyName("slots");
        JsonSerializer.Serialize(ref writer, value.GetEntityDataArray(), formatterResolver);
        writer.WriteValueSeparator();

        //Write recycled entity ids
        writer.WritePropertyName("recycledEntityIDs");
        writer.WriteBeginArray();
        var recycledEntityIDs = value.GetRecycledEntityIds();
        foreach (var recycledId in recycledEntityIDs)
        {
            writer.WriteBeginObject();
            writer.WritePropertyName("id");
            writer.WriteInt32(recycledId.Item1);
            writer.WriteValueSeparator();
            writer.WritePropertyName("version");
            writer.WriteInt32(recycledId.Item2);
            writer.WriteEndObject();

            writer.WriteValueSeparator();
        }
        // Cut last value seperator
        if (recycledEntityIDs.Count > 0)
        {
            writer.AdvanceOffset(-1);
        }
        writer.WriteEndArray();

        writer.WriteValueSeparator();

        //Write archetypes
        writer.WritePropertyName("archetypes");
        writer.WriteBeginArray();
        foreach (var archetype in value)
        {
            JsonSerializer.Serialize(ref writer, archetype, formatterResolver);
            writer.WriteValueSeparator();
        }

        // Cut last value seperator
        if (value.Archetypes.Count > 0)
        {
            writer.AdvanceOffset(-1);
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    public World Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        // Create world and setup formatter
        var archetypeFormatter = formatterResolver.GetFormatter<Archetype>() as ArchetypeFormatter;
        var entityFormatter = formatterResolver.GetFormatter<Entity>() as EntityFormatter;

        reader.ReadIsBeginObject();

        // Read meta data
        reader.ReadPropertyName();
        var baseChunkSize = reader.ReadUInt32();
        reader.ReadIsValueSeparator();
        
        reader.ReadPropertyName();
        var baseChunkEntityCount = reader.ReadUInt32();
        reader.ReadIsValueSeparator();
        
        // Construct world
        var world = World.Create(chunkSizeInBytes: (int)baseChunkSize, minimumAmountOfEntitiesPerChunk: (int)baseChunkEntityCount);
        entityFormatter.WorldId = world.Id;
        archetypeFormatter.World = world;
        
        // Read slots
        reader.ReadPropertyName();
        var slots = JsonSerializer.Deserialize<JaggedArray<EntityData>>(ref reader, formatterResolver);
        reader.ReadIsValueSeparator();

        // Read recycled ids
        var count = 0;
        List<(int, int)> recycledIds = new();

        reader.ReadPropertyName();
        reader.ReadIsBeginArray();

        while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
        {
            reader.ReadIsBeginObject();
            reader.ReadPropertyName();
            var id = reader.ReadInt32();
            reader.ReadIsValueSeparator();
            reader.ReadPropertyName();
            var value = reader.ReadInt32();
            reader.ReadIsEndObject();

            (int, int) recycledId = new(id, value);

            recycledIds.Add(recycledId);
        }

        reader.ReadIsValueSeparator();
        
        // Forward values to the world
        world.SetRecycledEntityIds(recycledIds);
        world.SetEntityDataArray(slots);
        world.EnsureCapacity(slots.Capacity);

        // Read archetypes
        count = 0;
        List<Archetype> archetypes = new();
        reader.ReadPropertyName();
        reader.ReadIsBeginArray();
        while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
        {
            var archetype = archetypeFormatter.Deserialize(ref reader, formatterResolver);
            archetypes.Add(archetype);
        }

        // Set archetypes
        world.SetArchetypes(archetypes);
        reader.ReadIsEndObject();
        return world;
    }
}

/// <summary>
///     The <see cref="ArchetypeFormatter"/> class
///     is a <see cref="IJsonFormatter{Archetype}"/> to (de)serialize <see cref="Archetype"/>s to or from json.
/// </summary>
public partial class ArchetypeFormatter : IJsonFormatter<Archetype>
{

    /// <summary>
    ///     The <see cref="World"/> which is being used by this formatter during serialisation/deserialisation. 
    /// </summary>
    internal World World { get; set; }

    public void Serialize(ref JsonWriter writer, Archetype value, IJsonFormatterResolver formatterResolver)
    {
        // Setup formatters
        var types = value.Signature;
        var chunks = value.Chunks;
        var chunkFormatter = formatterResolver.GetFormatter<Chunk>() as ChunkFormatter;
        chunkFormatter.Signature = types;

        writer.WriteBeginObject();

        // Write type array
        writer.WritePropertyName("types");
        JsonSerializer.Serialize(ref writer, types, formatterResolver);
        writer.WriteValueSeparator();

        // Write lookup array
        writer.WritePropertyName("lookup");
        JsonSerializer.Serialize(ref writer, value.GetLookupArray(), formatterResolver);
        writer.WriteValueSeparator();

        // Write chunk size
        writer.WritePropertyName("chunkCount");
        writer.WriteUInt32((uint)value.ChunkCount);
        writer.WriteValueSeparator();

        // Write chunks 
        writer.WritePropertyName("chunks");
        writer.WriteBeginArray();
        for (var index = 0; index < value.ChunkCount; index++)
        {
            ref var chunk = ref chunks[index];
            chunkFormatter.Serialize(ref writer, chunk, formatterResolver);
            writer.WriteValueSeparator();
        }

        // Trim last value separator
        if (value.ChunkCount > 0)
        {
            writer.AdvanceOffset(-1);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    public Archetype Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var chunkFormatter = formatterResolver.GetFormatter<Chunk>() as ChunkFormatter;

        reader.ReadIsBeginObject();

        // Types
        reader.ReadPropertyName();
        var types = JsonSerializer.Deserialize<Signature>(ref reader, formatterResolver);
        reader.ReadIsValueSeparator();

        // Archetype lookup array
        reader.ReadPropertyName();
        var lookupArray = JsonSerializer.Deserialize<int[]>(ref reader, formatterResolver);
        reader.ReadIsValueSeparator();

        // Archetype chunk size and list
        reader.ReadPropertyName();
        var chunkCount = reader.ReadUInt32();
        reader.ReadIsValueSeparator();

        // Create archetype
        var chunks = new List<Chunk>((int)chunkCount);
        var archetype = DangerousArchetypeExtensions.CreateArchetype(World.BaseChunkSize, World.BaseChunkEntityCount, types);
        archetype.Chunks.Clear(true);
        archetype.SetCount((int)chunkCount - 1);

        // Pass types and lookup array to the chunk formatter for saving performance and memory
        chunkFormatter.World = World;
        chunkFormatter.Archetype = archetype;
        chunkFormatter.Signature = types;
        chunkFormatter.LookupArray = lookupArray;

        // Deserialise each chunk and put it into the archetype. 
        reader.ReadPropertyName();
        reader.ReadIsBeginArray();

        var entities = 0;
        for (var index = 0; index < chunkCount; index++)
        {
            var chunk = chunkFormatter.Deserialize(ref reader, formatterResolver);
            chunks.Add(chunk);
            entities += chunk.Count;
            reader.ReadIsValueSeparator();
        }
        
        archetype.SetChunks(chunks);
        archetype.SetEntities(entities);

        reader.ReadIsEndArray();
        reader.ReadIsEndObject();
        return archetype;
    }
}

/// <summary>
///     The <see cref="ChunkFormatter"/> class
///     is a <see cref="IJsonFormatter{Chunk}"/> to (de)serialize <see cref="Chunk"/>s to or from json.
/// </summary>
public partial class ChunkFormatter : IJsonFormatter<Chunk>
{

    /// <summary>
    ///     The <see cref="Archetype"/> the current (de)serialized <see cref="Chunk"/> belongs to.
    ///     Since chunks do not know this, we need to pass this information along it. 
    /// </summary>
    internal World World { get; set; }

    /// <summary>
    ///     The <see cref="Archetype"/> the current (de)serialized <see cref="Chunk"/> belongs to.
    ///     Since chunks do not know this, we need to pass this information along it. 
    /// </summary>
    internal Archetype Archetype { get; set; }

    /// <summary>
    ///     The types used in the <see cref="Chunk"/> in each <see cref="Chunk"/> (de)serialized by this formatter.
    ///     <remarks>Since <see cref="Chunk"/> does not have a reference to them and its controlled by its <see cref="Archetype"/>.</remarks>
    /// </summary>
    internal Signature Signature { get; set; } = Signature.Null;

    /// <summary>
    ///     The lookup array used by each <see cref="Chunk"/> (de)serialized by this formatter.
    ///     <remarks>Since <see cref="Chunk"/> does not have a reference to them and its controlled by its <see cref="Archetype"/>.</remarks>
    /// </summary>
    internal int[] LookupArray { get; set; } = Array.Empty<int>();

    public void Serialize(ref JsonWriter writer, Chunk value, IJsonFormatterResolver formatterResolver)
    {
        writer.WriteBeginObject();

        // Write size
        writer.WritePropertyName("count");
        writer.WriteUInt32((uint)value.Count);
        writer.WriteValueSeparator();

        // Write capacity
        writer.WritePropertyName("capacity");
        writer.WriteUInt32((uint)value.Capacity);
        writer.WriteValueSeparator();

        // Write entitys
        writer.WritePropertyName("entities");
        JsonSerializer.NonGeneric.Serialize(ref writer, value.Entities, formatterResolver);
        writer.WriteValueSeparator();

        // Persist arrays as an array...
        writer.WritePropertyName("arrays");
        writer.WriteBeginArray();
        foreach(var type in Signature.Components)
        {
            // Write array itself
            var array = value.GetArray(type);
            JsonSerializer.Serialize(ref writer, array, formatterResolver);
            writer.WriteValueSeparator();
        }

        // Remove trailing 
        if (Signature.Count > 0)
        {
            writer.AdvanceOffset(-1);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    public Chunk Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        reader.ReadIsBeginObject();

        // Read chunk size
        reader.ReadPropertyName();
        var size = reader.ReadUInt32();
        reader.ReadIsValueSeparator();

        // Read chunk size
        reader.ReadPropertyName();
        var capacity = reader.ReadUInt32();
        reader.ReadIsValueSeparator();

        // Read entities
        reader.ReadPropertyName();
        var entities = JsonSerializer.Deserialize<Entity[]>(ref reader, formatterResolver);
        reader.ReadIsValueSeparator();

        // Create chunk
        var chunk = DangerousChunkExtensions.CreateChunk((int)capacity, LookupArray, Signature);
        entities.CopyTo(chunk.Entities, 0);
        chunk.SetSize((int)size);

        // Updating World.EntityInfoStorage to their new archetype
        for (var index = 0; index < size; index++)
        {
            ref var entity = ref chunk.Entity(index);
            entity = DangerousEntityExtensions.CreateEntityStruct(entity.Id, World.Id, entity.Version);
            World.SetArchetype(entity, Archetype);
        }

        // Persist arrays as an array...
        reader.ReadPropertyName();
        reader.ReadIsBeginArray();
        foreach(var type in Signature)
        {
            // Read array of the type
            var array = JsonSerializer.Deserialize<Array>(ref reader, formatterResolver);
            var chunkArray = chunk.GetArray(array.GetType().GetElementType());
            Array.Copy(array, chunkArray, (int)size);
            reader.ReadIsValueSeparator();
        }

        reader.ReadIsEndArray();
        reader.ReadIsEndObject();
        return chunk;
    }
}


