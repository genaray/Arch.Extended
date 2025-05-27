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
        private QueryDescription IncrementAAndB_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA), typeof(global::Arch.System.SourceGenerator.Tests.IntComponentB)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private World? _IncrementAAndB_Initialized;
        private Query? _IncrementAAndB_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementAAndBQuery(World world)
        {
            if (!ReferenceEquals(_IncrementAAndB_Initialized, world))
            {
                _IncrementAAndB_Query = world.Query(in IncrementAAndB_QueryDescription);
                _IncrementAAndB_Initialized = world;
            }

            foreach (ref var chunk in _IncrementAAndB_Query)
            {
                ref var entityFirstElement = ref chunk.Entity(0);
                foreach (var entityIndex in chunk)
                {
                    ref readonly var e = ref Unsafe.Add(ref entityFirstElement, entityIndex);
                    IncrementAAndB(@e);
                }
            }
        }
    }
}