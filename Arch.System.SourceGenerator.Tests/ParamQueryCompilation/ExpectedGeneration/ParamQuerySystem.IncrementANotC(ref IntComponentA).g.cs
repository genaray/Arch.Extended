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
    partial class ParamQuerySystem
    {
        private static QueryDescription IncrementANotC_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentC)), exclusive: Signature.Null);
        private static World? _IncrementANotC_Initialized;
        private static Query? _IncrementANotC_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IncrementANotCQuery(World world)
        {
            if (!ReferenceEquals(_IncrementANotC_Initialized, world))
            {
                _IncrementANotC_Query = world.Query(in IncrementANotC_QueryDescription);
                _IncrementANotC_Initialized = world;
            }

            foreach (ref var chunk in _IncrementANotC_Query)
            {
                ref var @intcomponentaFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentA>();
                foreach (var entityIndex in chunk)
                {
                    ref var @a = ref Unsafe.Add(ref intcomponentaFirstElement, entityIndex);
                    IncrementANotC(ref @a);
                }
            }
        }
    }
}