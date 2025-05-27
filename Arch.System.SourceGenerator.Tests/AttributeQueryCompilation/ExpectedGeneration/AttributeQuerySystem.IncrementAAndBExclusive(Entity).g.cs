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
        private QueryDescription IncrementAAndBExclusive_QueryDescription = new QueryDescription(all: Signature.Null, any: Signature.Null, none: Signature.Null, exclusive: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA), typeof(global::Arch.System.SourceGenerator.Tests.IntComponentB)));
        private World? _IncrementAAndBExclusive_Initialized;
        private Query? _IncrementAAndBExclusive_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementAAndBExclusiveQuery(World world)
        {
            if (!ReferenceEquals(_IncrementAAndBExclusive_Initialized, world))
            {
                _IncrementAAndBExclusive_Query = world.Query(in IncrementAAndBExclusive_QueryDescription);
                _IncrementAAndBExclusive_Initialized = world;
            }

            foreach (ref var chunk in _IncrementAAndBExclusive_Query)
            {
                ref var entityFirstElement = ref chunk.Entity(0);
                foreach (var entityIndex in chunk)
                {
                    ref readonly var e = ref Unsafe.Add(ref entityFirstElement, entityIndex);
                    IncrementAAndBExclusive(@e);
                }
            }
        }
    }
}