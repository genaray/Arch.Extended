using Arch.Core;
using MessagePack;
using MessagePack.Formatters;
using System.Buffers;
using Utf8Json;
using Utf8Json.Resolvers;
using DateTimeFormatter = Utf8Json.Formatters.DateTimeFormatter;
using NullableDateTimeFormatter = Utf8Json.Formatters.NullableDateTimeFormatter;

namespace Arch.Persistence;

/// <summary>
///     The <see cref="ArchSerializer"/> interface
///     represents an interface with shared methods to (de)serialize worlds and entities.
///     <remarks>It might happen that the serialized object is too large to fit into a regular c# byte-array. In this case use the <see cref="IBufferWriter{T}"/>-API.</remarks>
/// </summary>
public interface IArchSerializer
{
    /// <summary>
    ///     Serializes an <see cref="Entity"/> to a <see cref="byte"/>-array.
    /// </summary>
    /// <param name="world">The <see cref="World"/>.</param>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    byte[] Serialize(World world, Entity entity);

    /// <summary>
    ///     Serializes an <see cref="Entity"/> to a <see cref="Stream"/> e.g. a File or existing array.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/>.</param>
    /// <param name="world">The <see cref="World"/>.</param>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    void Serialize(Stream stream, World world, Entity entity);

    /// <summary>
    ///     Serializes an <see cref="Entity"/> to a <see cref="IBufferWriter{T}"/> e.g. a File or existing array.
    /// </summary>
    /// <param name="writer">The <see cref="IBufferWriter{T}"/>.</param>
    /// <param name="world">The <see cref="World"/>.</param>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    void Serialize(IBufferWriter<byte> writer, World world, Entity entity);

    /// <summary>
    ///     Deserializes an <see cref="Entity"/> from its bytes to an real <see cref="Entity"/> in a <see cref="World"/>.
    ///     <remarks>The new <see cref="Entity.Id"/> and <see cref="Entity.WorldId"/> will differ.</remarks>
    /// </summary>
    /// <param name="world">The <see cref="World"/>.</param>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    /// <returns></returns>
    Entity Deserialize(World world, byte[] entity);

    /// <summary>
    ///     Deserializes an <see cref="Entity"/> from its bytes to an real <see cref="Entity"/> in a <see cref="World"/>.
    ///     <remarks>The new <see cref="Entity.Id"/> and <see cref="Entity.WorldId"/> will differ.</remarks>
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/>.</param>
    /// <param name="world">The <see cref="World"/>.</param>
    /// <returns></returns>
    Entity Deserialize(Stream stream, World world);

    /// <summary>
    ///     Serializes a <see cref="World"/> to a <see cref="byte"/>-array.
    /// </summary>
    /// <param name="world">The <see cref="World"/>.</param>
    byte[] Serialize(World world);

    /// <summary>
    ///     Serializes a <see cref="World"/> to a <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/>.</param>
    /// <param name="world">The <see cref="World"/>.</param>
    void Serialize(Stream stream, World world);

    /// <summary>
    ///     Serializes a <see cref="World"/> to a <see cref="IBufferWriter{T}"/>.
    /// </summary>
    /// <param name="writer">The <see cref="IBufferWriter{T}"/>.</param>
    /// <param name="world">The <see cref="World"/>.</param>
    void Serialize(IBufferWriter<byte> writer, World world);

    /// <summary>
    ///     Deserializes a byte-array into a <see cref="World"/>.
    /// </summary>
    /// <param name="world">The <see cref="World"/> as an byte-array.</param>
    /// <returns>The new <see cref="World"/>.</returns>
    World Deserialize(byte[] world);

    /// <summary>
    ///     Deserializes a byte-array into a <see cref="World"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/>.</param>
    /// <returns>The new <see cref="World"/>.</returns>
    World Deserialize(Stream stream);
}

/// <summary>
///     The <see cref="ArchBinarySerializer"/> class
///     represents a binary serializer for arch to (de)serialize single entities and whole worlds by binary. 
/// </summary>
public class ArchBinarySerializer : IArchSerializer
{
    /// <summary>
    ///     The default formatters used to (de)serialize the <see cref="World"/>.
    /// </summary>
    private readonly IMessagePackFormatter[] _formatters =
    {
        new WorldFormatter(),
        new ArchetypeFormatter(),
        new ChunkFormatter(),
        new ArrayFormatter(),
        new ComponentTypeFormatter(),
        new SignatureFormatter(),
        new EntitySlotFormatter(),
        new EntityFormatter(),
        new JaggedArrayFormatter<int>(-1),
        new JaggedArrayFormatter<(int,int)>((-1,-1)),
        new JaggedArrayFormatter<EntityData>(new EntityData(null, new Slot(-1,-1)))
    };

    /// <summary>
    ///     The default formatters used to (de)serialize a single <see cref="Entity"/>.
    /// </summary>
    private readonly IMessagePackFormatter[] _singleEntityFormatters =
    {
        new ComponentTypeFormatter(),
        new SignatureFormatter(),
        new SingleEntityFormatter()
    };

    /// <summary>
    ///     The standard <see cref="MessagePackSerializerOptions"/> for world (de)serialization.
    /// </summary>
    private readonly MessagePackSerializerOptions _options;

    /// <summary>
    ///     The standard <see cref="MessagePackSerializerOptions"/> for single entity (de)serialization.
    /// </summary>
    private readonly MessagePackSerializerOptions _singleEntityOptions;

    /// <summary>
    ///     The static constructor gets called during compile time to setup the serializer. 
    /// </summary>
    public ArchBinarySerializer(params IMessagePackFormatter[] custFormatters)
    {
        // Register all important jsonformatters 
        _options = MessagePackSerializerOptions.Standard.WithResolver(
            MessagePack.Resolvers.CompositeResolver.Create(
                _formatters.Concat(custFormatters).ToList(),
                new List<IFormatterResolver>
                {
                    MessagePack.Resolvers.BuiltinResolver.Instance,
                    MessagePack.Resolvers.ContractlessStandardResolverAllowPrivate.Instance
                }
            )
        );

        _singleEntityOptions = MessagePackSerializerOptions.Standard.WithResolver(
            MessagePack.Resolvers.CompositeResolver.Create(
                _singleEntityFormatters.Concat(custFormatters).ToList(),
                new List<IFormatterResolver>
                {
                    MessagePack.Resolvers.BuiltinResolver.Instance,
                    MessagePack.Resolvers.ContractlessStandardResolverAllowPrivate.Instance
                }
            )
        );
    }

    /// <inheritdoc/>
    public byte[] Serialize(World world, Entity entity)
    {
        (_singleEntityFormatters[2] as SingleEntityFormatter)!.EntityWorld = world;
        return MessagePackSerializer.Serialize(entity, _singleEntityOptions);
    }

    /// <inheritdoc/>
    public void Serialize(Stream stream, World world, Entity entity)
    {
        (_singleEntityFormatters[2] as SingleEntityFormatter)!.EntityWorld = world;
        MessagePackSerializer.Serialize(stream, entity, _singleEntityOptions);
    }

    /// <inheritdoc/>
    public void Serialize(IBufferWriter<byte> writer, World world, Entity entity)
    {
        (_singleEntityFormatters[2] as SingleEntityFormatter)!.EntityWorld = world;
        MessagePackSerializer.Serialize(writer, entity, _singleEntityOptions);
    }

    /// <inheritdoc/>
    public Entity Deserialize(World world, byte[] entity)
    {
        (_singleEntityFormatters[2] as SingleEntityFormatter)!.EntityWorld = world;
        return MessagePackSerializer.Deserialize<Entity>(entity, _singleEntityOptions);
    }

    /// <inheritdoc/>
    public Entity Deserialize(Stream stream, World world)
    {
        (_singleEntityFormatters[2] as SingleEntityFormatter)!.EntityWorld = world;
        return MessagePackSerializer.Deserialize<Entity>(stream, _singleEntityOptions);
    }

    /// <inheritdoc/>
    public byte[] Serialize(World world)
    {
        return MessagePackSerializer.Serialize(world, _options); ;
    }

    /// <inheritdoc/>
    public void Serialize(Stream stream, World world)
    {
        MessagePackSerializer.Serialize(stream, world, _options);
    }

    /// <inheritdoc/>
    public void Serialize(IBufferWriter<byte> writer, World world)
    {
        MessagePackSerializer.Serialize(writer, world, _options);
    }

    /// <inheritdoc/>
    public World Deserialize(byte[] world)
    {
        return MessagePackSerializer.Deserialize<World>(world, _options);
    }

    /// <inheritdoc/>
    public World Deserialize(Stream stream)
    {
        return MessagePackSerializer.Deserialize<World>(stream, _options);
    }
}

/// <summary>
///     The <see cref="ArchJsonSerializer"/> class
///     represents a json serializer for arch to (de)serialize single entities and whole worlds by binary. 
/// </summary>
public class ArchJsonSerializer : IArchSerializer
{

    /// <summary>
    ///     The default formatters used to (de)serialize the <see cref="World"/>.
    /// </summary>
    private readonly IJsonFormatter[] _formatters = {
        new WorldFormatter(),
        new ArchetypeFormatter(),
        new ChunkFormatter(),
        new ArrayFormatter(),
        new ComponentTypeFormatter(),
        new SignatureFormatter(),
        new EntitySlotFormatter(),
        new EntityFormatter(),
        new JaggedArrayFormatter<int>(-1),
        new JaggedArrayFormatter<(int,int)>((-1,-1)),
        new JaggedArrayFormatter<EntityData>(new EntityData(null, new Slot(-1, -1))),
        new DateTimeFormatter("yyyy-MM-dd HH:mm:ss"),
        new NullableDateTimeFormatter("yyyy-MM-dd HH:mm:ss")
    };

    /// <summary>
    ///     The default formatters used to (de)serialize a single <see cref="Entity"/>.
    /// </summary>
    private readonly IJsonFormatter[] _singleEntityFormatters =
    {
        new ComponentTypeFormatter(),
        new SignatureFormatter(),
        new SingleEntityFormatter(),
        new DateTimeFormatter("yyyy-MM-dd HH:mm:ss"),
        new NullableDateTimeFormatter("yyyy-MM-dd HH:mm:ss")
    };

    // It can `not` garbage collect and create is slightly high cost.
    // so you should store to static field.
    private IJsonFormatterResolver _formatterResolver;

    // CompositeResolver.Create can create dynamic composite resolver.
    // It can `not` garbage collect and create is slightly high cost.
    // so you should store to static field.
    private IJsonFormatterResolver _singleEntityFormatterResolver;

    /// <summary>
    ///     The static constructor gets called during compile time to setup the serializer. 
    /// </summary>
    public ArchJsonSerializer(params IJsonFormatter[] custFormatters)
    {
        // Register all important jsonformatters 
        _formatterResolver = CompositeResolver.Create(
            _formatters.Concat(custFormatters).ToArray(),
            new[] {
                EnumResolver.UnderlyingValue,
                StandardResolver.AllowPrivateExcludeNullSnakeCase,
                BuiltinResolver.Instance,
                DynamicGenericResolver.Instance,
            }
        );

        _singleEntityFormatterResolver = CompositeResolver.Create(
            _singleEntityFormatters.Concat(custFormatters).ToArray(),
            new[] {
                EnumResolver.UnderlyingValue,
                StandardResolver.AllowPrivateExcludeNullSnakeCase,
            }
        );
    }

    /// <summary>
    ///     The static constructor gets called during compile time to setup the serializer. 
    ///     This variant allows custom resolvers to be passed in as well.
    /// </summary>
    public ArchJsonSerializer(IJsonFormatter[] custFormatters, IJsonFormatterResolver[] custResolvers)
    {
        // Register all important jsonformatters 
        _formatterResolver = CompositeResolver.Create(
            _formatters.Concat(custFormatters).ToArray(),
            custResolvers.Concat(new[] {
                EnumResolver.UnderlyingValue,
                StandardResolver.AllowPrivateExcludeNullSnakeCase,
                BuiltinResolver.Instance,
                DynamicGenericResolver.Instance,
            }).ToArray()
        );

        _singleEntityFormatterResolver = CompositeResolver.Create(
            _singleEntityFormatters.Concat(custFormatters).ToArray(),
            custResolvers.Concat(new[] {
                EnumResolver.UnderlyingValue,
                StandardResolver.AllowPrivateExcludeNullSnakeCase,
            }).ToArray()
        );
    }

    /// <summary>
    ///     Serializes the given <see cref="World"/> to a json-string.
    /// </summary>
    /// <param name="world">The <see cref="World"/> to serialize.</param>
    /// <returns>Its json-string.</returns>
    public string ToJson(World world)
    {
        return JsonSerializer.ToJsonString(world, _formatterResolver);
    }

    /// <summary>
    ///     Serializes the given <see cref="Entity"/> to a json-string.
    /// </summary>
    /// <param name="world">The <see cref="World"/> the entity belongs to..</param>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    /// <returns>Its json-string.</returns>
    /// <returns>A json-string of the entity with all its components.</returns>
    public string ToJson(World world, Entity entity)
    {
        (_singleEntityFormatters[2] as SingleEntityFormatter)!.EntityWorld = world;
        return JsonSerializer.ToJsonString(entity, _singleEntityFormatterResolver);
    }

    /// <summary>
    ///     Deserializes the given json <see cref="string"/> to a <see cref="World"/>.
    /// </summary>
    /// <param name="jsonWorld">The json <see cref="string"/> to deserialize.</param>
    /// <returns>A new <see cref="World"/>.</returns>
    public World FromJson(string jsonWorld)
    {
        return JsonSerializer.Deserialize<World>(jsonWorld, _formatterResolver);
    }

    /// <summary>
    ///     Deserializes the given json <see cref="string"/> to a <see cref="World"/>.
    ///     <remarks>The deserialized <see cref="Entity"/> will receive a new id and a new worldId.</remarks>
    /// </summary>
    /// <param name="world">The <see cref="World"/> to deserialize the entity into.</param>
    /// <param name="jsonEntity">The json <see cref="string"/> of the entity to deserialize.</param>
    /// <returns>A new <see cref="Entity"/>.</returns>
    public Entity FromJson(World world, string jsonEntity)
    {
        (_singleEntityFormatters[2] as SingleEntityFormatter)!.EntityWorld = world;
        return JsonSerializer.Deserialize<Entity>(jsonEntity, _singleEntityFormatterResolver);
    }

    /// <inheritdoc/>
    public byte[] Serialize(World world, Entity entity)
    {
        (_singleEntityFormatters[2] as SingleEntityFormatter)!.EntityWorld = world;
        return JsonSerializer.Serialize(entity, _singleEntityFormatterResolver);
    }

    /// <inheritdoc/>
    public void Serialize(Stream stream, World world, Entity entity)
    {
        (_singleEntityFormatters[1] as SingleEntityFormatter)!.EntityWorld = world;
        JsonSerializer.Serialize(stream, entity, _singleEntityFormatterResolver);
    }

    /// <inheritdoc/>
    public void Serialize(IBufferWriter<byte> writer, World world, Entity entity)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Entity Deserialize(World world, byte[] entity)
    {
        (_singleEntityFormatters[2] as SingleEntityFormatter)!.EntityWorld = world;
        return JsonSerializer.Deserialize<Entity>(entity, _singleEntityFormatterResolver);
    }

    /// <inheritdoc/>
    public Entity Deserialize(Stream stream, World world)
    {
        (_singleEntityFormatters[2] as SingleEntityFormatter)!.EntityWorld = world;
        return JsonSerializer.Deserialize<Entity>(stream, _singleEntityFormatterResolver);
    }

    /// <inheritdoc/>
    public byte[] Serialize(World world)
    {
        return JsonSerializer.Serialize(world, _formatterResolver);
    }

    /// <inheritdoc/>
    public void Serialize(Stream stream, World world)
    {
        JsonSerializer.Serialize(stream, world, _formatterResolver);
    }

    /// <inheritdoc/>
    public void Serialize(IBufferWriter<byte> writer, World world)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public World Deserialize(byte[] world)
    {
        return JsonSerializer.Deserialize<World>(world, _formatterResolver);
    }

    /// <inheritdoc/>
    public World Deserialize(Stream stream)
    {
        return JsonSerializer.Deserialize<World>(stream, _formatterResolver);
    }
}