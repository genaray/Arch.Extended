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
        private static QueryDescription IncrementOnlyAWithB_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA), typeof(global::Arch.System.SourceGenerator.Tests.IntComponentB)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private static World? _IncrementOnlyAWithB_Initialized;
        private static Query? _IncrementOnlyAWithB_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IncrementOnlyAWithBQuery(World world)
        {
            if (!ReferenceEquals(_IncrementOnlyAWithB_Initialized, world))
            {
                _IncrementOnlyAWithB_Query = world.Query(in IncrementOnlyAWithB_QueryDescription);
                _IncrementOnlyAWithB_Initialized = world;
            }

            foreach (ref var chunk in _IncrementOnlyAWithB_Query)
            {
                ref var @intcomponentaFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentA>();
                ref var @intcomponentbFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentB>();
                foreach (var entityIndex in chunk)
                {
                    ref var @a = ref Unsafe.Add(ref intcomponentaFirstElement, entityIndex);
                    ref var @_ = ref Unsafe.Add(ref intcomponentbFirstElement, entityIndex);
                    IncrementOnlyAWithB(ref @a, in @_);
                }
            }
        }
    }
}