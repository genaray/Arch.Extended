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
        private QueryDescription IncrementAOrBNotC_QueryDescription = new QueryDescription(all: Signature.Null, any: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA), typeof(global::Arch.System.SourceGenerator.Tests.IntComponentB)), none: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentC)), exclusive: Signature.Null);
        private World? _IncrementAOrBNotC_Initialized;
        private Query? _IncrementAOrBNotC_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementAOrBNotCQuery(World world)
        {
            if (!ReferenceEquals(_IncrementAOrBNotC_Initialized, world))
            {
                _IncrementAOrBNotC_Query = world.Query(in IncrementAOrBNotC_QueryDescription);
                _IncrementAOrBNotC_Initialized = world;
            }

            foreach (ref var chunk in _IncrementAOrBNotC_Query)
            {
                ref var entityFirstElement = ref chunk.Entity(0);
                foreach (var entityIndex in chunk)
                {
                    ref readonly var e = ref Unsafe.Add(ref entityFirstElement, entityIndex);
                    IncrementAOrBNotC(@e);
                }
            }
        }
    }
}