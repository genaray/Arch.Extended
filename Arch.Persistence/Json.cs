using Arch.Core;
using Arch.Core.Extensions.Dangerous;
using Arch.Core.Utils;
using Utf8Json;

namespace Arch.Persistence;

// WORK IN PROGRESS

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
        var objectFormatter = formatterResolver.GetFormatter<object>();

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

public class WorldFormatter : IJsonFormatter<World>
{

    public void Serialize(ref JsonWriter writer, World value, IJsonFormatterResolver formatterResolver)
    {
        var archetypes = value.Archetypes;
        var archetypeFormatter = formatterResolver.GetFormatter<IList<Archetype>>();
        var intArrayFormatter = formatterResolver.GetFormatter<int[][]>();
        var slotFormatter = formatterResolver.GetFormatter<(int,int)[][]>();
        
        writer.WriteBeginObject();
        
        // Write world property, the amount of entities in it
        writer.WritePropertyName("size");
        writer.WriteUInt32((uint)value.Size);
        writer.WriteValueSeparator();
        
        // Write entity info
        writer.WritePropertyName("versions");
        intArrayFormatter.Serialize(ref writer, value.GetVersions(), formatterResolver);
        writer.WriteValueSeparator();
        
        // Write slots
        writer.WritePropertyName("slots");
        slotFormatter.Serialize(ref writer, value.GetSlots(), formatterResolver);
        writer.WriteValueSeparator();

        // Write archetypes
        writer.WritePropertyName("archetypesSize");
        writer.WriteUInt32((uint)archetypes.Count);
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
        
        reader.ReadIsBeginObject();
        
        reader.ReadPropertyName();
        var size = reader.ReadUInt32();
        reader.ReadIsValueSeparator();

        reader.ReadPropertyName();
        var versions = intArrayFormatter.Deserialize(ref reader, formatterResolver);
        reader.ReadIsValueSeparator();
        
        reader.ReadPropertyName();
        var slots = slotFormatter.Deserialize(ref reader, formatterResolver);
        reader.ReadIsValueSeparator();
        
        reader.ReadPropertyName();
        var archetypeSize = reader.ReadUInt32();
        reader.ReadIsValueSeparator();

        reader.ReadPropertyName();
        var archetypes = archetypeFormatter.Deserialize(ref reader, formatterResolver);
        world.SetArchetypes(archetypes);
        reader.ReadIsValueSeparator();
        
        // Forward values to the world 
        world.SetVersions(versions);
        world.SetSlots(slots);
        
        reader.ReadIsEndObject();
        return world;
    }
}

public class ArchetypeFormatter : IJsonFormatter<Archetype>
{
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

        // Write amount of entities 
        writer.WritePropertyName("size");
        writer.WriteUInt32((uint)value.Entities);
        writer.WriteValueSeparator();
        
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

        // Archetype size
        reader.ReadPropertyName();
        var size = reader.ReadUInt32();
        reader.ReadIsValueSeparator();
        
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

public class ChunkFormatter : IJsonFormatter<Chunk>
{
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
        var objectFormatter = formatterResolver.GetFormatter<Array>();
        
        writer.WriteBeginObject();
        
        // Write size
        writer.WritePropertyName("size");
        writer.WriteUInt32((uint)value.Size);
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
            
            objectFormatter.Serialize(ref writer, array, formatterResolver);
            //objectFormatter.Serialize(ref writer, array, formatterResolver);

            writer.WriteEndObject();
            writer.WriteValueSeparator();
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
    }

    public Chunk Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var typeFormatter = formatterResolver.GetFormatter<Type>();
        //var objectFormatter = formatterResolver.GetFormatter<Array>();

        reader.ReadIsBeginObject();

        // chunk size
        reader.ReadPropertyName();
        var size = reader.ReadUInt32();
        reader.ReadIsValueSeparator();
        
        // Create chunk
        var chunk = DangerousChunkExtensions.CreateChunk((int)size, LookupArray, Types);

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
            var array = (Array)JsonSerializer.NonGeneric.Deserialize(typeof(Array), ref reader);
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


