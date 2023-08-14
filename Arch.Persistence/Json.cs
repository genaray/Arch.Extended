using Arch.Core;
using Arch.Core.Extensions;
using Arch.Core.Extensions.Dangerous;
using Arch.Core.Utils;
using Utf8Json;
using Utf8Json.Formatters;
using Utf8Json.Resolvers;

namespace Arch.Persistence;

public static class ArchSerializer
{

    private static SingleEntityFormatter SingleEntityFormatter { get; } = new ();
    
    /// <summary>
    ///     The static constructor gets called during compile time to setup the serializer. 
    /// </summary>
    public static void Initialize(params IJsonFormatter[] formatters)
    {
        var requiredFormatters = new IJsonFormatter[]
        {
            new WorldFormatter(),
            new ArchetypeFormatter(),
            new ChunkFormatter(),
            new ComponentTypeFormatter(),
            new ArrayFormatter(),
            new SlotFormatter(),
            new DateTimeFormatter("yyyy-MM-dd HH:mm:ss"),
            new NullableDateTimeFormatter("yyyy-MM-dd HH:mm:ss")
        };
        
        // Register all important jsonformatters 
        CompositeResolver.RegisterAndSetAsDefault(
            requiredFormatters.Concat(formatters).ToArray(), 
            new[] {
                EnumResolver.UnderlyingValue,
                StandardResolver.AllowPrivateExcludeNullSnakeCase
            }
        );
    }

    /// <summary>
    ///     Serializes the given <see cref="World"/> to a json-string.
    /// </summary>
    /// <param name="world">The <see cref="World"/> to serialize.</param>
    /// <returns>Its json-string.</returns>
    public static string Serialize(World world)
    {
        return JsonSerializer.ToJsonString(world);
    }

    /*
    public static string Serialize(World world, Entity entity)
    {
        SingleEntityFormatter.World = world;
        var writer = new JsonWriter(JsonSerializer..GetBuffer());
        return SingleEntityFormatter.Serialize(ref writer, entity, CompositeResolver.Instance);
    }*/
    
    /// <summary>
    ///     Deserializes the given json <see cref="string"/> to a <see cref="World"/>.
    /// </summary>
    /// <param name="jsonWorld">The json <see cref="string"/> to deserialize.</param>
    /// <returns>A new <see cref="World"/>.</returns>
    public static World Deserialize(string jsonWorld)
    {
        return JsonSerializer.Deserialize<World>(jsonWorld);
    }
}

/// <summary>
///     The <see cref="SingleEntityFormatter"/> class
///     is a <see cref="IJsonFormatter{Entity}"/> to (de)serialize a single <see cref="Entity"/>to or from json.
/// </summary>
public class SingleEntityFormatter : IJsonFormatter<Entity>
{
    
    /// <summary>
    ///     The <see cref="World"/> the entity belongs to. 
    /// </summary>
    public World World { get; set; }
    
    public void Serialize(ref JsonWriter writer, Entity value, IJsonFormatterResolver formatterResolver)
    {
        writer.WriteBeginObject();
        
        // Write id
        writer.WritePropertyName("id");
        writer.WriteInt32(value.Id);
        writer.WriteValueSeparator();
        
        // Write world
        writer.WritePropertyName("worldId");
        writer.WriteInt32(value.WorldId);
        writer.WriteValueSeparator();
        
        // Write components
        var componentTypes = value.GetComponentTypes();
        writer.WriteBeginArray();
        foreach (ref var type in componentTypes.AsSpan())
        {
            var cmp = value.Get(type);
            JsonSerializer.NonGeneric.Serialize(ref writer, cmp, formatterResolver);
            writer.WriteValueSeparator();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    public Entity Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        reader.ReadIsBeginObject();

        reader.ReadPropertyName();
        var entityId = reader.ReadInt32();
        
        
        reader.ReadIsEndObject();
        return Entity.Null;
    }
}

/// <summary>
///     The <see cref="SlotFormatter"/> class
///     is a <see cref="IJsonFormatter{Tuple}"/> to (de)serialize <see cref="Slot"/>-<see cref="Tuple{T,K}"/>s to or from json.
/// </summary>
public class SlotFormatter : IJsonFormatter<ValueTuple<int, int>>
{
    public void Serialize(ref JsonWriter writer, ValueTuple<int, int> value, IJsonFormatterResolver formatterResolver)
    {
        writer.WriteBeginObject();
        writer.WriteInt32(value.Item1);
        writer.WriteValueSeparator();
        writer.WriteInt32(value.Item2);
        writer.WriteEndObject();
    }

    public ValueTuple<int, int> Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        reader.ReadIsBeginObject();
        var itemOne = reader.ReadInt32();
        reader.ReadIsValueSeparator();
        var itemTwo = reader.ReadInt32();
        reader.ReadIsEndObject();

        return new(itemOne, itemTwo);
    }
}

/// <summary>
///     The <see cref="ArrayFormatter"/> class
///     is a <see cref="IJsonFormatter{Array}"/> to (de)serialize <see cref="Array"/>s to or from json.
/// </summary>
public class ArrayFormatter : IJsonFormatter<Array>
{
    public void Serialize(ref JsonWriter writer, Array value, IJsonFormatterResolver formatterResolver)
    {
        var typeFormatter = formatterResolver.GetFormatter<Type>();
        var type = value.GetType().GetElementType();
        
        // Write type and size
        writer.WriteBeginObject();
        writer.WritePropertyName("type");
        typeFormatter.Serialize(ref writer, type, formatterResolver);
        writer.WriteValueSeparator();
        
        writer.WritePropertyName("size");
        writer.WriteUInt32((uint)value.Length);
        writer.WriteValueSeparator();
        
        // Write array
        writer.WriteBeginArray();
        for (var index = 0; index < value.Length; index++)
        {
            var obj = value.GetValue(index);
            JsonSerializer.NonGeneric.Serialize(ref writer, obj, formatterResolver);
            writer.WriteValueSeparator();
        }
        writer.WriteEndArray();
    }

    public Array Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var typeFormatter = formatterResolver.GetFormatter<Type>();
     
        // Write type and size
        reader.ReadIsBeginObject();
        reader.ReadPropertyName();
        var type = typeFormatter.Deserialize(ref reader, formatterResolver);
        reader.ReadIsValueSeparator();
        
        reader.ReadPropertyName();
        var size = reader.ReadUInt32();
        reader.ReadIsValueSeparator();
        
        // Create array
        var array = Array.CreateInstance(type, size);
        
        // Read array
        reader.ReadIsBeginArray();
        for (var index = 0; index < size; index++)
        {
            var obj = JsonSerializer.NonGeneric.Deserialize(type, ref reader, formatterResolver);
            array.SetValue(obj, index);
            reader.ReadIsValueSeparator();
        }
        reader.ReadIsEndArray();
        return array;
    }
}

/// <summary>
///     The <see cref="ComponentTypeFormatter"/> class
///     is a <see cref="IJsonFormatter{ComponentType}"/> to (de)serialize <see cref="ComponentType"/>s to or from json.
/// </summary>
public class ComponentTypeFormatter : IJsonFormatter<ComponentType>
{
    public void Serialize(ref JsonWriter writer, ComponentType value, IJsonFormatterResolver formatterResolver)
    {
        writer.WriteBeginObject();
        
        // Write id
        writer.WritePropertyName("id");
        writer.WriteUInt32((uint)value.Id);
        writer.WriteValueSeparator();
        
        // Write type itself
        writer.WritePropertyName("type");
        formatterResolver.GetFormatter<Type>().Serialize(ref writer, value.Type, formatterResolver);
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
        var type = formatterResolver.GetFormatter<Type>().Deserialize(ref reader, formatterResolver);
        reader.ReadIsValueSeparator();

        reader.ReadPropertyName();
        var bytesize = reader.ReadUInt32();
        reader.ReadIsValueSeparator();
        
        reader.ReadIsEndObject();

        return new ComponentType((int)id, type, (int)bytesize, false);
    }
}

/// <summary>
///     The <see cref="WorldFormatter"/> class
///     is a <see cref="IJsonFormatter{World}"/> to (de)serialize <see cref="World"/>s to or from json.
/// </summary>
public class WorldFormatter : IJsonFormatter<World>
{
    public void Serialize(ref JsonWriter writer, World value, IJsonFormatterResolver formatterResolver)
    {
        var archetypes = value.Archetypes;
        var archetypeFormatter = formatterResolver.GetFormatter<IList<Archetype>>();
        var intArrayFormatter = formatterResolver.GetFormatter<int[][]>();
        var slotFormatter = formatterResolver.GetFormatter<(int,int)[][]>();
        
        writer.WriteBeginObject();

        // Write entity info
        writer.WritePropertyName("versions");
        intArrayFormatter.Serialize(ref writer, value.GetVersions(), formatterResolver);
        writer.WriteValueSeparator();
        
        // Write slots
        writer.WritePropertyName("slots");
        slotFormatter.Serialize(ref writer, value.GetSlots(), formatterResolver);
        writer.WriteValueSeparator();

        writer.WritePropertyName("archetypes");
        archetypeFormatter.Serialize(ref writer, archetypes, formatterResolver);
        writer.WriteValueSeparator();

        writer.WriteEndObject();
    }

    public World Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var world = World.Create();
        var intArrayFormatter = formatterResolver.GetFormatter<int[][]>();
        var slotFormatter = formatterResolver.GetFormatter<(int,int)[][]>();
        var archetypeFormatter = formatterResolver.GetFormatter<List<Archetype>>();
        ArchetypeFormatter.World = world;
        
        reader.ReadIsBeginObject();

        reader.ReadPropertyName();
        var versions = intArrayFormatter.Deserialize(ref reader, formatterResolver);
        reader.ReadIsValueSeparator();
        
        reader.ReadPropertyName();
        var slots = slotFormatter.Deserialize(ref reader, formatterResolver);
        reader.ReadIsValueSeparator();

        reader.ReadPropertyName();
        var archetypes = archetypeFormatter.Deserialize(ref reader, formatterResolver);
        world.SetArchetypes(archetypes);
        reader.ReadIsValueSeparator();
        
        // Forward values to the world 
        world.SetVersions(versions);
        world.SetSlots(slots);
        world.EnsureCapacity(versions.Length);

        reader.ReadIsEndObject();
        return world;
    }
}

/// <summary>
///     The <see cref="ArchetypeFormatter"/> class
///     is a <see cref="IJsonFormatter{Archetype}"/> to (de)serialize <see cref="Archetype"/>s to or from json.
/// </summary>
public class ArchetypeFormatter : IJsonFormatter<Archetype>
{
    
    /// <summary>
    ///     The <see cref="World"/> which is being used by this formatter during serialisation/deserialisation. 
    /// </summary>
    public static World World { get; set; }
    
    public void Serialize(ref JsonWriter writer, Archetype value, IJsonFormatterResolver formatterResolver)
    {
        // Get formatters
        var types = value.Types;
        var typeFormatter = formatterResolver.GetFormatter<ComponentType[]>();
        var lookupFormatter = formatterResolver.GetFormatter<int[]>();
        
        var chunks = value.Chunks;
        var chunkFormatter = formatterResolver.GetFormatter<Chunk>() as ChunkFormatter;
        chunkFormatter.Types = types;
        
        writer.WriteBeginObject();

        // Write type array
        writer.WritePropertyName("types");
        typeFormatter.Serialize(ref writer, types, formatterResolver);
        writer.WriteValueSeparator();

        // Write lookup array
        writer.WritePropertyName("lookup");
        lookupFormatter.Serialize(ref writer, value.GetLookupArray(), formatterResolver);
        writer.WriteValueSeparator();
        
        // Write chunk size
        writer.WritePropertyName("chunkSize");
        writer.WriteUInt32((uint)value.Size);
        writer.WriteValueSeparator();

        // Write chunks 
        writer.WritePropertyName("chunks");
        writer.WriteBeginArray();
        for (var index = 0; index < types.Length; index++)
        {
            ref var chunk = ref chunks[index];
            chunkFormatter.Serialize(ref writer, chunk, formatterResolver);
            writer.WriteValueSeparator();
        }
        writer.WriteEndArray();
        
        writer.WriteEndObject();
    }

    public Archetype Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        
        var typeFormatter = formatterResolver.GetFormatter<ComponentType[]>();
        var chunkFormatter = formatterResolver.GetFormatter<Chunk>() as ChunkFormatter;
        var lookupFormatter = formatterResolver.GetFormatter<int[]>();

        reader.ReadIsBeginObject();

        // Types
        reader.ReadPropertyName();
        var types = typeFormatter.Deserialize(ref reader, formatterResolver);
        reader.ReadIsValueSeparator();

        // Archetype lookup array
        reader.ReadPropertyName();
        var lookupArray = lookupFormatter.Deserialize(ref reader, formatterResolver);
        reader.ReadIsValueSeparator();
        
        // Archetype chunk size and list
        reader.ReadPropertyName();
        var chunkSize = reader.ReadUInt32();
        var chunks = new List<Chunk>((int)chunkSize);
        reader.ReadIsValueSeparator();

        // Create archetype
        var archetype = DangerousArchetypeExtensions.CreateArchetype(types.ToArray());
        archetype.SetSize((int)chunkSize);
        
        // Pass types and lookup array to the chunk formatter for saving performance and memory
        chunkFormatter.World = World;
        chunkFormatter.Archetype = archetype;
        chunkFormatter.Types = types;
        chunkFormatter.LookupArray = lookupArray;
        
        // Deserialise each chunk and put it into the archetype. 
        reader.ReadPropertyName();
        reader.ReadIsBeginArray();
        for (var index = 0; index < chunkSize; index++)
        {
            var chunk = chunkFormatter.Deserialize(ref reader, formatterResolver);
            chunks.Add(chunk);
            reader.ReadIsValueSeparator();
        }
        archetype.SetChunks(chunks);
        reader.ReadIsEndArray();
        reader.ReadIsValueSeparator();
        
        reader.ReadIsEndObject();
        return archetype;
    }
}

/// <summary>
///     The <see cref="ChunkFormatter"/> class
///     is a <see cref="IJsonFormatter{Chunk}"/> to (de)serialize <see cref="Chunk"/>s to or from json.
/// </summary>
public class ChunkFormatter : IJsonFormatter<Chunk>
{
    
    /// <summary>
    ///     The <see cref="Archetype"/> the current (de)serialized <see cref="Chunk"/> belongs to.
    ///     Since chunks do not know this, we need to pass this information along it. 
    /// </summary>
    public World World { get; set; }
    
    /// <summary>
    ///     The <see cref="Archetype"/> the current (de)serialized <see cref="Chunk"/> belongs to.
    ///     Since chunks do not know this, we need to pass this information along it. 
    /// </summary>
    public Archetype Archetype { get; set; }
    
    /// <summary>
    ///     The types used in the <see cref="Chunk"/> in each <see cref="Chunk"/> (de)serialized by this formatter.
    ///     <remarks>Since <see cref="Chunk"/> does not have a reference to them and its controlled by its <see cref="Archetype"/>.</remarks>
    /// </summary>
    public ComponentType[] Types { get; set; } = Array.Empty<ComponentType>();

    /// <summary>
    ///     The lookup array used by each <see cref="Chunk"/> (de)serialized by this formatter.
    ///     <remarks>Since <see cref="Chunk"/> does not have a reference to them and its controlled by its <see cref="Archetype"/>.</remarks>
    /// </summary>
    public int[] LookupArray { get; set; } = Array.Empty<int>();
    
    public void Serialize(ref JsonWriter writer, Chunk value, IJsonFormatterResolver formatterResolver)
    {
        var typeFormatter = formatterResolver.GetFormatter<Type>();
        var arrayFormatter = formatterResolver.GetFormatter<Array>();
 
        writer.WriteBeginObject();
        
        // Write size
        writer.WritePropertyName("size");
        writer.WriteUInt32((uint)value.Size);
        writer.WriteValueSeparator();
        
        // Write capacity
        writer.WritePropertyName("capacity");
        writer.WriteUInt32((uint)value.Capacity);
        writer.WriteValueSeparator();
        
        // Write entitys
        writer.WritePropertyName("entitys");
        JsonSerializer.NonGeneric.Serialize(ref writer, value.Entities, formatterResolver);
        writer.WriteValueSeparator();

        // Persist arrays as an array...
        writer.WritePropertyName("arrays");
        writer.WriteBeginArray();
        for (var index = 0; index < Types.Length; index++)
        {
            // Write type
            ref var type = ref Types[index];
            writer.WriteBeginObject();
            writer.WritePropertyName("type");
            typeFormatter.Serialize(ref writer, type.Type, formatterResolver);
            writer.WriteValueSeparator();
            
            // Write array itself
            writer.WritePropertyName("array");
            var array = value.GetArray(type);
            
            arrayFormatter.Serialize(ref writer, array, formatterResolver);
  
            writer.WriteEndObject();
            writer.WriteValueSeparator();
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
    }

    public Chunk Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var typeFormatter = formatterResolver.GetFormatter<Type>();        
        var arrayFormatter = formatterResolver.GetFormatter<Array>();
  
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
        var entities = (Entity[])JsonSerializer.NonGeneric.Deserialize(typeof(Entity[]), ref reader, formatterResolver);
        reader.ReadIsValueSeparator();
        
        // Create chunk
        var chunk = DangerousChunkExtensions.CreateChunk((int)capacity, LookupArray, Types);
        entities.CopyTo(chunk.Entities,0);
        chunk.SetSize((int)size);

        // Updating World.EntityInfoStorage to their new archetype
        for (var index = 0; index < entities.Length; index++)
        {
            ref var entity = ref chunk.Entity(index);
            entity = DangerousEntityExtensions.CreateEntityStruct(entity.Id, World.Id);
            World.SetArchetype(entity, Archetype);
        }
        
        // Persist arrays as an array...
        reader.ReadPropertyName();
        reader.ReadIsBeginArray();
        for (var index = 0; index < Types.Length; index++)
        {
            // Read type of the following array
            reader.ReadIsBeginObject();
            reader.ReadPropertyName();
            var type = typeFormatter.Deserialize(ref reader, formatterResolver);
            reader.ReadIsValueSeparator();
            
            // Read array of the type
            reader.ReadPropertyName();
            var array = arrayFormatter.Deserialize(ref reader, formatterResolver);
            var chunkArray = chunk.GetArray(type);
            Array.Copy(array,chunkArray, (int)size);
            reader.ReadIsEndObject();
            reader.ReadIsValueSeparator();
        }
        
        reader.ReadIsEndArray();
        reader.ReadIsEndObject();
        return chunk;
    }
}


