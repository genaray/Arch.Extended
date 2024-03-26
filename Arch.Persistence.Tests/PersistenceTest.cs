using Arch.Core;
using Arch.Core.Extensions;
using CommunityToolkit.HighPerformance;
using static NUnit.Framework.Assert;
using Throws = NUnit.Framework.Throws;

namespace Arch.Persistence.Tests;

/// <summary>
///     The <see cref="Transform"/> struct
///     represents a test component that is being (de)serialized.
/// </summary>
public record struct Transform
{
    public float X;
    public float Y;
}

/// <summary>
///     The <see cref="MetaData"/> struct
///     represents a test component that is being (de)serialized.
/// </summary>
public record struct MetaData
{
    public string Name;
}

public class Tests
{

    private ArchBinarySerializer _binarySerializer;
    private ArchJsonSerializer _jsonSerializer;
    private World _world;
    
    [SetUp]
    public void Setup()
    {
        _binarySerializer = new ArchBinarySerializer();
        _jsonSerializer = new ArchJsonSerializer();
        
        _world = World.Create();
        for (var index = 0; index < 1000; index++)
        {
            _world.Create(new Transform { X = index, Y = index }, new MetaData{ Name = index.ToString()});
        }
    }

    /// <summary>
    ///     Checks if a world is being serialized and deserialized correctly using the <see cref="_binarySerializer"/>.
    /// </summary>
    [Test]
    public void BinaryWorldSerialization()
    {
        var bytes = _binarySerializer.Serialize(_world);
        var newWorld = _binarySerializer.Deserialize(bytes);
        
        // Equal in structure?
        That(newWorld.Capacity, Is.EqualTo(_world.Capacity));
        That(newWorld.Size, Is.EqualTo(_world.Size));
        That(newWorld.Archetypes.Count, Is.EqualTo(_world.Archetypes.Count));

        // Are archetypes equal?
        for (var index = 0; index < _world.Archetypes.Count; index++)
        {
            var archetype = _world.Archetypes[index];
            var newArchetype = newWorld.Archetypes[index];
            
            That(archetype.ChunkCapacity, Is.EqualTo(newArchetype.ChunkCapacity));
            That(archetype.EntityCount, Is.EqualTo(newArchetype.EntityCount));
        }
        
        // Are entities equal?
        var entities = new Entity[_world.Size];
        _world.GetEntities(new QueryDescription().WithNone<int>(), entities.AsSpan());
        
        var newEntities = new Entity[newWorld.Size];
        newWorld.GetEntities(new QueryDescription(), newEntities.AsSpan());

        for (var index = 0; index < entities.Length; index++)
        {
            var entity = entities[index];
            var newEntity = newEntities[index];
            
            That(entity.Id, Is.EqualTo(newEntity.Id));
            That(entity.Get<Transform>(), Is.EqualTo(newEntity.Get<Transform>()));
            That(entity.Get<MetaData>(), Is.EqualTo(newEntity.Get<MetaData>()));
        }
    }

    /// <summary>
    ///     Checks if an entity is being serialized and deserialized correctly using the <see cref="_binarySerializer"/>.
    /// </summary>
    [Test]
    public void BinaryEntitySerialization()
    {
        var entity = _world.Archetypes[0].GetChunk(0).Entity(0);
        var bytes = _binarySerializer.Serialize(_world, entity);

        var newWorld = World.Create();
        var newEntity = _binarySerializer.Deserialize(newWorld, bytes);
        
        That(newEntity.Get<Transform>(), Is.EqualTo(entity.Get<Transform>()));
        That(newEntity.Get<MetaData>(), Is.EqualTo(entity.Get<MetaData>()));
    }

    /// <summary>
    ///     Checks if a world is being serialized and deserialized correctly using the <see cref="_jsonSerializer"/>.
    /// </summary>
    [Test]
    public void JsonWorldSerialization()
    {
        var bytes = _jsonSerializer.Serialize(_world);
        var newWorld = _jsonSerializer.Deserialize(bytes);
        
        // Equal in structure?
        That(newWorld.Capacity, Is.EqualTo(_world.Capacity));
        That(newWorld.Size, Is.EqualTo(_world.Size));
        That(newWorld.Archetypes.Count, Is.EqualTo(_world.Archetypes.Count));

        // Are archetypes equal?
        for (var index = 0; index < _world.Archetypes.Count; index++)
        {
            var archetype = _world.Archetypes[index];
            var newArchetype = newWorld.Archetypes[index];
            
            That(archetype.ChunkCapacity, Is.EqualTo(newArchetype.ChunkCapacity));
            That(archetype.EntityCount, Is.EqualTo(newArchetype.EntityCount));
        }
        
        // Are entities equal?
        var entities = new Entity[_world.Size];
        _world.GetEntities(new QueryDescription().WithNone<int>(), entities.AsSpan());
        
        var newEntities = new Entity[newWorld.Size];
        newWorld.GetEntities(new QueryDescription(), newEntities.AsSpan());

        for (var index = 0; index < entities.Length; index++)
        {
            var entity = entities[index];
            var newEntity = newEntities[index];
            
            That(entity.Id, Is.EqualTo(newEntity.Id));
            That(entity.Get<Transform>(), Is.EqualTo(newEntity.Get<Transform>()));
            That(entity.Get<MetaData>(), Is.EqualTo(newEntity.Get<MetaData>()));
        }
    }
    
    
    /// <summary>
    ///     Checks if an entity is being serialized and deserialized correctly using the <see cref="_binarySerializer"/>.
    /// </summary>
    [Test]
    public void JsonEntitySerialization()
    {
        var entity = _world.Archetypes[0].GetChunk(0).Entity(0);
        var bytes = _jsonSerializer.Serialize(_world, entity);

        var newWorld = World.Create();
        var newEntity = _jsonSerializer.Deserialize(newWorld, bytes);
        
        That(newEntity.Get<Transform>(), Is.EqualTo(entity.Get<Transform>()));
        That(newEntity.Get<MetaData>(), Is.EqualTo(entity.Get<MetaData>()));
    }
}