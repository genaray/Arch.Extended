using System.Runtime.CompilerServices;
using Arch.Core;
using static NUnit.Framework.Assert;
using Throws = NUnit.Framework.Throws;

namespace Arch.Relationships.Tests;

//#if EVENTS

public record struct ParentOf;
public record struct ChildOf;

/// <summary>
///     The <see cref="RelationshipTest"/> class
///     contains several tests to check if the relations work correctly. 
/// </summary>
[TestFixture]
public class RelationshipTest
{
    private World _world = default!;

    [SetUp]
    public void SetUp()
    {
        _world = World.Create();
    }

    /// <summary>
    ///     Checks if no relationships are handled corretly. 
    /// </summary>
    [Test]
    public void NoRelationships()
    {
        // Create entities without setting any relationships
        var parent = _world.Create();
        var childOne = _world.Create();
        var childTwo = _world.Create();

        // Get should throw
        That(() => _world.GetRelationships<ParentOf>(parent), Throws.Exception);
        That(() => _world.GetRelationships<ChildOf>(childOne), Throws.Exception);
        That(() => _world.GetRelationships<ChildOf>(childTwo), Throws.Exception);

        // TryGet should return false
        False(_world.TryGetRelationships<ParentOf>(parent, out _));
        False(_world.TryGetRelationships<ChildOf>(childOne, out _));
        False(_world.TryGetRelationships<ChildOf>(childTwo, out _));

        // TryGetRef should return a null ref and false
        That(Unsafe.IsNullRef(ref _world.TryGetRefRelationships<ParentOf>(parent, out var exists)));
        False(exists);

        That(Unsafe.IsNullRef(ref _world.TryGetRefRelationships<ChildOf>(childOne, out exists)));
        False(exists);

        That(Unsafe.IsNullRef(ref _world.TryGetRefRelationships<ChildOf>(childTwo, out exists)));
        False(exists);
    }

    /// <summary>
    ///     Checks if relationships were added correctly.
    /// </summary>
    [Test]
    public void AddRelationship()
    {
        var source = _world.Create();
        var target = _world.Create();
        
        source.AddRelationship<ParentOf>(target);
        That(source.HasRelationship<ParentOf>(), Is.True);
        That(source.HasRelationship<ParentOf>(target), Is.True);
    }
    
    /// <summary>
    ///     Checks if relationships were set correctly.
    /// </summary>
    [Test]
    public void RelationshipData()
    {
        var source = _world.Create();
        var target = _world.Create();
        var dummy = _world.Create();
        
        source.AddRelationship(target, 5);
        source.AddRelationship(dummy, 100);
        
        var data = source.GetRelationship<int>(target);
        That(data, Is.EqualTo(5));
        
        source.SetRelationship(target, 10);
        data = source.GetRelationship<int>(target);
        That(data, Is.EqualTo(10));
        
        var tower = _world.Create();
        var battle = _world.Create(new int(), new long());

        // Expected result:
        // A relationship should be created, with an apparently useless target parameter, which should hold a component with the information I actually want
        battle.AddRelationship<int>(battle);

        var deadBattles = new QueryDescription().WithAll<int, long>();

        _world.Query(in deadBattles, (in Entity battle) =>
        {

            // System.MissingMethodException: 'Method not found: 'System.Collections.Generic.List`1<Arch.Core.World> Arch.Core.World.get_Worlds()'.'
            var tower = battle.GetRelationships<Entity>();
        });
    }
    
    /// <summary>
    ///     Checks if relationships were removed correctly.
    /// </summary>
    [Test]
    public void RemoveRelationship()
    {
        var source = _world.Create();
        var target = _world.Create();
        
        source.AddRelationship<ParentOf>(target);

        That(source.HasRelationship<ParentOf>(), Is.True);
        That(source.HasRelationship<ParentOf>(target), Is.True);
        
        source.RemoveRelationship<ParentOf>(target);
        That(source.HasRelationship<ParentOf>(), Is.False);
    }

#if EVENTS

    [Test]
    public void RelationshipCleanup()
    {
        // Setup handling relationship cleanup
        _world.HandleRelationshipCleanup();

        // Create entities
        var parent = _world.Create();
        var childOne = _world.Create();
        var childTwo = _world.Create();

        // Add the relationships
        _world.AddRelationship<ParentOf>(parent, childOne);
        _world.AddRelationship<ParentOf>(parent, childTwo);
        _world.AddRelationship<ChildOf>(childOne, parent);
        _world.AddRelationship<ChildOf>(childTwo, parent);

        // Get should return a reference to the relationships
        That(_world.GetRelationships<ParentOf>(parent).Elements, Does
            .ContainKey(childOne).And
            .ContainKey(childTwo));
        That(_world.GetRelationships<ChildOf>(childOne).Elements, Does.ContainKey(parent));
        That(_world.GetRelationships<ChildOf>(childTwo).Elements, Does.ContainKey(parent));

        // TryGet should return true and the relationships
        True(_world.TryGetRelationships<ParentOf>(parent, out var parentRelationships));
        That(parentRelationships.Elements, Does.ContainKey(childOne).And.ContainKey(childTwo));

        True(_world.TryGetRelationships<ChildOf>(childOne, out var childRelationships));
        That(childRelationships.Elements, Does.ContainKey(parent));

        True(_world.TryGetRelationships<ChildOf>(childTwo, out childRelationships));
        That(childRelationships.Elements, Does.ContainKey(parent));

        // TryGetRef should return a reference to the relationships and true
        ref var parentRelationshipsRef = ref _world.TryGetRefRelationships<ParentOf>(parent, out var exists);
        True(exists);
        That(parentRelationshipsRef.Elements, Does.ContainKey(childOne).And.ContainKey(childTwo));

        ref var childOneRelationshipsRef = ref _world.TryGetRefRelationships<ChildOf>(childOne, out exists);
        True(exists);
        That(childOneRelationshipsRef.Elements, Does.ContainKey(parent));

        ref var childTwoRelationshipsRef = ref _world.TryGetRefRelationships<ChildOf>(childTwo, out exists);
        True(exists);
        That(childTwoRelationshipsRef.Elements, Does.ContainKey(parent));

        // Destroy childOne, should remove any relationships containing it
        _world.Destroy(childOne);

        // Get should throw on childOne and not return it in any of the other references
        That(_world.GetRelationships<ParentOf>(parent).Elements, Does.Not.ContainKey(childOne).And.ContainKey(childTwo));
        That(() => _world.GetRelationships<ChildOf>(childOne).Elements, Throws.Exception);
        That(_world.GetRelationships<ChildOf>(childTwo).Elements, Does.ContainKey(parent));

        // Destroy childTwo, should remove any relationships containing it
        _world.Destroy(childTwo);

        // Get, all should throw as no relationships are left
        That(() => _world.GetRelationships<ParentOf>(parent), Throws.Exception);
        That(() => _world.GetRelationships<ChildOf>(childOne), Throws.Exception);
        That(() => _world.GetRelationships<ChildOf>(childTwo), Throws.Exception);

        // Destroy parent
        DoesNotThrow(() => _world.Destroy(parent));
    }
#endif

    [Test]
    public void QueryRelationship()
    {
        // Create entities
        var parent = _world.Create();
        var childOne = _world.Create();
        var childTwo = _world.Create();

        // Add relationships
        _world.AddRelationship<ParentOf>(parent, childOne);
        _world.AddRelationship<ParentOf>(parent, childTwo);
        _world.AddRelationship<ChildOf>(childOne, parent);
        _world.AddRelationship<ChildOf>(childTwo, parent);

        // Query all ParentOf relationships
        var query = new QueryDescription().WithAll<Relationship<ParentOf>>();
        var entities = new List<Entity>();
        _world.Query(query, (in Entity _, ref Relationship<ParentOf> parentOf) =>
        {
            entities.AddRange(parentOf.Elements.Select(p => p.Key));
        });

        // Should find two ParentOf relationships, from childOne and childTwo
        That(entities, Has.Count.EqualTo(2));
        That(entities, Does.Contain(childOne));
        That(entities, Does.Contain(childTwo));
    }
}

//#endif
