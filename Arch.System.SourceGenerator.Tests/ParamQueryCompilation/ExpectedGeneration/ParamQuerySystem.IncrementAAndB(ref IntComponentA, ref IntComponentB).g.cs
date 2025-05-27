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
        private static QueryDescription IncrementAAndB_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA), typeof(global::Arch.System.SourceGenerator.Tests.IntComponentB)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private static World? _IncrementAAndB_Initialized;
        private static Query? _IncrementAAndB_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IncrementAAndBQuery(World world)
        {
            if (!ReferenceEquals(_IncrementAAndB_Initialized, world))
            {
                _IncrementAAndB_Query = world.Query(in IncrementAAndB_QueryDescription);
                _IncrementAAndB_Initialized = world;
            }

            foreach (ref var chunk in _IncrementAAndB_Query)
            {
                ref var @intcomponentaFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentA>();
                ref var @intcomponentbFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentB>();
                foreach (var entityIndex in chunk)
                {
                    ref var @a = ref Unsafe.Add(ref intcomponentaFirstElement, entityIndex);
                    ref var @b = ref Unsafe.Add(ref intcomponentbFirstElement, entityIndex);
                    IncrementAAndB(ref @a, ref @b);
                }
            }
        }
    }
}