using System.IO.Compression;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Bus;
using Arch.Core.Extensions.Dangerous;
using Arch.Core.Utils;
using Arch.Persistence;
using Arch.Relationships;
using MessagePack;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NUnit.Framework;
using Schedulers;
using Utf8Json;
using Utf8Json.Formatters;
using Utf8Json.Resolvers;

namespace Arch.Extended;

/// <summary>
///     The <see cref="Game"/> which represents the game and implements all the important monogame features.
/// </summary>
public class Game : Microsoft.Xna.Framework.Game
{
    // The world and a job scheduler for multithreading
    private World _world;
    private JobScheduler _jobScheduler;
    
    // Our systems processing entities
    private System.Group<GameTime> _systems;
    private DrawSystem _drawSystem;

    // Monogame stuff
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _texture2D;
    private Random _random;
    
    public Game()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
    }
    
    protected override void Initialize()
    {
        // Setup texture and randomness
        _random = new Random();
        _texture2D = TextureExtensions.CreateSquareTexture(GraphicsDevice, 10);

        base.Initialize();
    }
    
    protected override void LoadContent()
    {
        // Create a new SpriteBatch, which can be used to draw textures.
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }
    
    protected override void BeginRun()
    {
        base.BeginRun();
        
        // Create world & JobScheduler for multithreading
        _world = World.Create();
        _jobScheduler = new(
            new JobScheduler.Config
            {
                ThreadPrefixName = "Arch.Samples",
                ThreadCount = 0,
                MaxExpectedConcurrentJobs = 64,
                StrictAllocationMode = false,
            }
        );
        World.SharedJobScheduler = _jobScheduler;
        
        // Spawn in entities with position, velocity and sprite
        for (var index = 0; index < 10; index++)
        {
            _world.Create(
                new Position{ Vector2 = _random.NextVector2(GraphicsDevice.Viewport.Bounds) }, 
                new Velocity{ Vector2 = _random.NextVector2(-0.25f,0.25f) }, 
                new Sprite{ Texture2D = _texture2D, Color = _random.NextColor() }
            );
        }
        
        //Serializer is not updated yet. 
        // Serialize world and deserialize it back. Just for showcasing the serialization, its actually not necessary.
        // var archSerializer = new ArchJsonSerializer(new SpriteSerializer{GraphicsDevice = GraphicsDevice});
        // var worldJson = archSerializer.ToJson(_world);
        // _world = archSerializer.FromJson(worldJson);
        
        var archSerializer = new ArchBinarySerializer(new SpriteSerializer{GraphicsDevice = GraphicsDevice});
        var worldJson = archSerializer.Serialize(_world);
        _world = archSerializer.Deserialize(worldJson);
        
        // Create systems, running in order
        _systems = new System.Group<GameTime>(
            "Systems",
            new MovementSystem(_world, GraphicsDevice.Viewport.Bounds),
            new ColorSystem(_world),
            new DebugSystem(_world)
        );
        _drawSystem = new DrawSystem(_world, _spriteBatch);  // Draw system must be its own system since monogame differentiates between update and draw. 
        
        // Initialize systems
        _systems.Initialize();
        _drawSystem.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        // Exit game on press
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();
        
        // Forward keyboard state as an event to another handles by using the eventbus
        var @event = (_world, Keyboard.GetState());
        EventBus.Send(ref @event);
        
         // Continue entity movement by adding velocity to all
        if (Keyboard.GetState().IsKeyDown(Keys.I))
        {
            // Query for velocity entities and remove their velocity to make them stop moving.
            var queryDesc = new QueryDescription().WithNone<Velocity>();
            _world.Add(in queryDesc, new Velocity { Vector2 = _random.NextVector2(-0.25f, 0.25f) });
        }

        // Pause entity movement by removing velocity from all
        if (Keyboard.GetState().IsKeyDown(Keys.O))
        {
            // Query for velocity entities and remove their velocity to make them stop moving.
            var queryDesc = new QueryDescription().WithAll<Velocity>();
            _world.Remove<Velocity>(in queryDesc);
        }

        // Add a random amount of new entities
        if (Keyboard.GetState().IsKeyDown(Keys.K))
        {
            // Bulk create entities
            var amount = Random.Shared.Next(0, 500);
            Span<Entity> entities = stackalloc Entity[amount];
            _world.Create(entities,[typeof(Position), typeof(Velocity), typeof(Sprite)], amount);

            // Set variables
            foreach (var entity in entities)
            { 

#if DEBUG_PUREECS || RELEASE_PUREECS
                _world.Set(entity,
                    new Position { Vector2 = _random.NextVector2(GraphicsDevice.Viewport.Bounds) },
                    new Velocity { Vector2 = _random.NextVector2(-0.25f, 0.25f) },
                    new Sprite { Texture2D = _texture2D, Color = _random.NextColor() }
                );
#else
                entity.Set(
                    new Position { Vector2 = _random.NextVector2(GraphicsDevice.Viewport.Bounds) },
                    new Velocity { Vector2 = _random.NextVector2(-0.25f, 0.25f) },
                    new Sprite { Texture2D = _texture2D, Color = _random.NextColor() }
                );
#endif
            }
        }

        // Remove a random amount of new entities
        if (Keyboard.GetState().IsKeyDown(Keys.L))
        {
            // Find all entities
            var entities = new Entity[_world.Size];
            _world.GetEntities(new QueryDescription(), entities.AsSpan());

            // Delete random entities
            var amount = Random.Shared.Next(0, Math.Min(500, entities.Length));
            for (var index = 0; index < amount; index++)
            {
                var randomIndex = _random.Next(0, entities.Length);
                var randomEntity = entities[randomIndex];

#if DEBUG_PUREECS || RELEASE_PUREECS
                if (_world.IsAlive(randomEntity))
#else
                if (randomEntity.IsAlive())
#endif
                {
                    _world.Destroy(randomEntity);
                }

                entities[randomIndex] = Entity.Null;
            }
        }
        
        // Update systems
        _systems.BeforeUpdate(in gameTime);
        _systems.Update(in gameTime);
        _systems.AfterUpdate(in gameTime);
        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        _graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
        
        // Update draw system and draw stuff
        _drawSystem.BeforeUpdate(in gameTime);
        _drawSystem.Update(in gameTime);
        _drawSystem.AfterUpdate(in gameTime);
        base.Draw(gameTime);
    }

    protected override void EndRun()
    {
        base.EndRun();
        
        // Destroy world and shutdown the jobscheduler 
        World.Destroy(_world);
        _jobScheduler.Dispose();
        
        // Dispose systems
        _systems.Dispose();
    }
}