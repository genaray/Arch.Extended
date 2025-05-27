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
        private QueryDescription IncrementA_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private World? _IncrementA_Initialized;
        private Query? _IncrementA_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementAQuery(World world)
        {
            if (!ReferenceEquals(_IncrementA_Initialized, world))
            {
                _IncrementA_Query = world.Query(in IncrementA_QueryDescription);
                _IncrementA_Initialized = world;
            }

            foreach (ref var chunk in _IncrementA_Query)
            {
                ref var entityFirstElement = ref chunk.Entity(0);
                foreach (var entityIndex in chunk)
                {
                    ref readonly var e = ref Unsafe.Add(ref entityFirstElement, entityIndex);
                    IncrementA(@e);
                }
            }
        }
    }
}