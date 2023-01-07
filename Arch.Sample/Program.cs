// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;
using Arch.Core;
using Arch.System;
using Test;

Console.WriteLine("Hello, World!");

var world = World.Create();
world.Create(new Position(0,0), new Velocity(1,1));
world.Create(new Position(0,0), new Velocity(1,1));
world.Create(new Position(0,0), new Velocity(1,1));

var group = new Group<float>(
    new MovementSystem(world),
    new DebugSystem(world)
);
group.Initialize(10.0f);
group.BeforeUpdate(10.0f);
group.Update(10.0f);
group.AfterUpdate(10.0f);
group.Dispose();