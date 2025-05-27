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
        private QueryDescription IncrementAOrB_QueryDescription = new QueryDescription(all: Signature.Null, any: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA), typeof(global::Arch.System.SourceGenerator.Tests.IntComponentB)), none: Signature.Null, exclusive: Signature.Null);
        private World? _IncrementAOrB_Initialized;
        private Query? _IncrementAOrB_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementAOrBQuery(World world)
        {
            if (!ReferenceEquals(_IncrementAOrB_Initialized, world))
            {
                _IncrementAOrB_Query = world.Query(in IncrementAOrB_QueryDescription);
                _IncrementAOrB_Initialized = world;
            }

            foreach (ref var chunk in _IncrementAOrB_Query)
            {
                ref var entityFirstElement = ref chunk.Entity(0);
                foreach (var entityIndex in chunk)
                {
                    ref readonly var e = ref Unsafe.Add(ref entityFirstElement, entityIndex);
                    IncrementAOrB(@e);
                }
            }
        }
    }
}