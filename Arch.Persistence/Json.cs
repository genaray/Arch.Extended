using Arch.Core;
using Arch.Core.Utils;
using Utf8Json;

namespace Arch.Persistence;

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
        throw new NotImplementedException();
    }
}

public class WorldFormatter : IJsonFormatter<World>
{

    public void Serialize(ref JsonWriter writer, World value, IJsonFormatterResolver formatterResolver)
    {
        var archetypes = value.Archetypes;
        var archetypeFormatter = formatterResolver.GetFormatter<IList<Archetype>>();

        writer.WriteBeginObject();
        
        // Write world property, the amount of entities in it
        writer.WritePropertyName("size");
        writer.WriteUInt32((uint)value.Size);
        writer.WriteValueSeparator();
        
        // Write entity info
        //writer.WritePropertyName("entityInfos");

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
        throw new NotImplementedException();
    }
}

public class ArchetypeFormatter : IJsonFormatter<Archetype>
{
    public void Serialize(ref JsonWriter writer, Archetype value, IJsonFormatterResolver formatterResolver)
    {
        // Get formatters
        var types = value.Types;
        var typeFormatter = formatterResolver.GetFormatter<ComponentType[]>();

        var chunks = value.Chunks;
        var chunkFormatter = formatterResolver.GetFormatter<Chunk>() as ChunkFormatter;
        chunkFormatter.Types = types;
        
        // Write type array
        writer.WritePropertyName("types");
        typeFormatter.Serialize(ref writer, types, formatterResolver);
        writer.WriteValueSeparator();
        
        // Write amount of entities 
        writer.WritePropertyName("size");
        writer.WriteUInt32((uint)value.Entities);
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
        throw new NotImplementedException();
    }
}

public class ChunkFormatter : IJsonFormatter<Chunk>
{
    public ComponentType[] Types { get; set; } = Array.Empty<ComponentType>();
    
    public void Serialize(ref JsonWriter writer, Chunk value, IJsonFormatterResolver formatterResolver)
    {

        var typeFormatter = formatterResolver.GetFormatter<Type>();
        var objectFormatter = formatterResolver.GetFormatter<object>();
        
        writer.WriteBeginObject();
        
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

            writer.WriteEndObject();
            writer.WriteValueSeparator();
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
    }

    public Chunk Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        throw new NotImplementedException();
    }
}


