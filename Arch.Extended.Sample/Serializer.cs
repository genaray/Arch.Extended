using MessagePack;
using MessagePack.Formatters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Utf8Json;

namespace Arch.Extended;

/// <summary>
///     The <see cref="SpriteSerializer"/> class
///     is a <see cref="IJsonFormatter{T}"/> for (de)serialising a <see cref="Sprite"/>.
/// </summary>
public class SpriteSerializer : IJsonFormatter<Sprite>, IMessagePackFormatter<Sprite>
{
    /// <summary>
    ///     The <see cref="GraphicsDevice"/> to create <see cref="Texture2D"/>s from. 
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; set; } = null!;

    public void Serialize(ref JsonWriter writer, Sprite value, IJsonFormatterResolver formatterResolver)
    {
        writer.WriteBeginObject();
        
        // Write color
        writer.WritePropertyName("color");
        writer.WriteUInt32(value.Color.PackedValue);
        writer.WriteValueSeparator();
        
        // Write texture id
        writer.WritePropertyName("textureId");
        writer.WriteUInt16(value.TextureId);

        writer.WriteEndObject();
    }

    public Sprite Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        reader.ReadIsBeginObject();
        
        // Read color
        reader.ReadPropertyName();
        var packedColor = reader.ReadUInt32();
        reader.ReadIsValueSeparator();
        
        // Read textureid
        reader.ReadPropertyName();
        var textureId = reader.ReadUInt16();

        // Create color and texture
        var color = new Color { PackedValue = packedColor };
        var texture = textureId switch
        {
            1 => TextureExtensions.CreateSquareTexture(GraphicsDevice, 10),
            _ => TextureExtensions.CreateSquareTexture(GraphicsDevice, 10)
        };

        reader.ReadIsEndObject();
        return new Sprite(texture, color);
    }

    public void Serialize(ref MessagePackWriter writer, Sprite value, MessagePackSerializerOptions options)
    {
        // Write color
        writer.WriteUInt32(value.Color.PackedValue);

        // Write texture id
        writer.WriteUInt16(value.TextureId);
    }

    public Sprite Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        // Read color
        var packedColor = reader.ReadUInt32();
   
        // Read textureid
        var textureId = reader.ReadUInt16();

        // Create color and texture
        var color = new Color { PackedValue = packedColor };
        var texture = textureId switch
        {
            1 => TextureExtensions.CreateSquareTexture(GraphicsDevice, 10),
            _ => TextureExtensions.CreateSquareTexture(GraphicsDevice, 10)
        };
        
        return new Sprite(texture, color);
    }
}