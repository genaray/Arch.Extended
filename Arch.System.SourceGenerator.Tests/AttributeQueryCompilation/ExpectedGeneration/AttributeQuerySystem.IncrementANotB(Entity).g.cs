#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Core.Utils;
using ArrayExtensions = CommunityToolkit.HighPerformance.ArrayExtensions;
using Component = Arch.Core.Component;

namespace Arch.System.SourceGenerator.Tests
{
    partial class AttributeQuerySystem
    {
        private QueryDescription IncrementANotB_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentB)), exclusive: Signature.Null);
        private World? _IncrementANotB_Initialized;
        private Query? _IncrementANotB_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementANotBQuery(World world)
        {
            if (!ReferenceEquals(_IncrementANotB_Initialized, world))
            {
                _IncrementANotB_Query = world.Query(in IncrementANotB_QueryDescription);
                _IncrementANotB_Initialized = world;
            }

            foreach (ref var chunk in _IncrementANotB_Query)
            {
                ref var entityFirstElement = ref chunk.Entity(0);
                foreach (var entityIndex in chunk)
                {
                    ref readonly var e = ref Unsafe.Add(ref entityFirstElement, entityIndex);
                    IncrementANotB(@e);
                }
            }
        }
    }
}