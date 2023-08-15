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
    
    /// <summary>
    ///     The default formatters used to (de)serialize the <see cref="World"/>.
    /// </summary>
    private static IJsonFormatter[] Formatters = {
        new WorldFormatter(),
        new ArchetypeFormatter(),
        new ChunkFormatter(),
        new ComponentTypeFormatter(),
        new ArrayFormatter(),
        new DateTimeFormatter("yyyy-MM-dd HH:mm:ss"),
        new NullableDateTimeFormatter("yyyy-MM-dd HH:mm:ss")
    };

    /// <summary>
    ///     The default formatters used to (de)serialize a single <see cref="Entity"/>.
    /// </summary>
    private static IJsonFormatter[] SingleEntityFormatters =
    {
        new ComponentTypeFormatter(),
        new SingleEntityFormatter(),
        new DateTimeFormatter("yyyy-MM-dd HH:mm:ss"),
        new NullableDateTimeFormatter("yyyy-MM-dd HH:mm:ss")
    };
    
    // CompositeResolver.Create can create dynamic composite resolver.
    // It can `not` garbage collect and create is slightly high cost.
    // so you should store to static field.
    public static IJsonFormatterResolver SingleEntityFormatterResolver;
    
    /// <summary>
    ///     The static constructor gets called during compile time to setup the serializer. 
    /// </summary>
    public static void Initialize(params IJsonFormatter[] custFormatters)
    {
        // Register all important jsonformatters 
        CompositeResolver.RegisterAndSetAsDefault(
            Formatters.Concat(custFormatters).ToArray(), 
            new[] {
                EnumResolver.UnderlyingValue,
                StandardResolver.AllowPrivateExcludeNullSnakeCase
            }
        );
        
        SingleEntityFormatterResolver = CompositeResolver.Create(
            SingleEntityFormatters.Concat(custFormatters).ToArray(),
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

    /// <summary>
    ///     Serializes the given <see cref="Entity"/> to a json-string.
    /// </summary>
    /// <param name="world">The <see cref="World"/> the entity belongs to..</param>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    /// <returns>Its json-string.</returns>
    /// <returns>A json-string of the entity with all its components.</returns>
    public static string Serialize(World world, Entity entity)
    {
        SingleEntityFormatter.EntityWorld = world;
        return JsonSerializer.ToJsonString(entity, SingleEntityFormatterResolver);
    }
    
    /// <summary>
    ///     Deserializes the given json <see cref="string"/> to a <see cref="World"/>.
    /// </summary>
    /// <param name="jsonWorld">The json <see cref="string"/> to deserialize.</param>
    /// <returns>A new <see cref="World"/>.</returns>
    public static World Deserialize(string jsonWorld)
    {
        return JsonSerializer.Deserialize<World>(jsonWorld);
    }
    
    /// <summary>
    ///     Deserializes the given json <see cref="string"/> to a <see cref="World"/>.
    /// </summary>
    /// <param name="world">The <see cref="World"/> to deserialize the entity into.</param>
    /// <param name="jsonEntity">The json <see cref="string"/> of the entity to deserialize.</param>
    /// <returns>A new <see cref="Entity"/>.</returns>
    public static Entity Deserialize(World world, string jsonEntity)
    {
        SingleEntityFormatter.EntityWorld = world;
        return JsonSerializer.Deserialize<Entity>(jsonEntity, SingleEntityFormatterResolver);
    }
}