using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Utf8Json;

namespace Arch.Extended;

/// <summary>
///     The <see cref="SpriteSerializer"/> class
///     is a <see cref="IJsonFormatter{T}"/> for (de)serialising a <see cref="Sprite"/>.
/// </summary>
public class SpriteSerializer : IJsonFormatter<Sprite>
{
    /// <summary>
    ///     The <see cref="GraphicsDevice"/> to create <see cref="Texture2D"/>s from. 
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; set; }
    
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
        writer.WriteValueSeparator();
        
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
        reader.ReadIsValueSeparator();
        
        // Create color and texture
        var color = new Color { PackedValue = packedColor };
        Texture2D texture = textureId switch
        {
            1 => TextureExtensions.CreateSquareTexture(GraphicsDevice, 10),
            _ => TextureExtensions.CreateSquareTexture(GraphicsDevice, 10)
        };

        reader.ReadIsEndObject();
        return new Sprite(texture, color);
    }
}