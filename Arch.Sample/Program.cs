// See https://aka.ms/new-console-template for more information

using Arch.Core;
using Test;

Console.WriteLine("Hello, World!");

var world = World.Create();
world.Create(new Position(0,0), new Velocity(1,1));
world.Create(new Position(0,0), new Velocity(1,1));
world.Create(new Position(0,0), new Velocity(1,1));

var bc = new MovemmentSystem(world);
bc.Update(10.0f);