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
    partial class DataParamSystem
    {
        private static QueryDescription CountATwiceWithParams_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA), typeof(global::Arch.System.SourceGenerator.Tests.IntComponentB)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private static World? _CountATwiceWithParams_Initialized;
        private static Query? _CountATwiceWithParams_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CountATwiceWithParamsQuery(World world, ref int @count1, ref int @count2)
        {
            if (!ReferenceEquals(_CountATwiceWithParams_Initialized, world))
            {
                _CountATwiceWithParams_Query = world.Query(in CountATwiceWithParams_QueryDescription);
                _CountATwiceWithParams_Initialized = world;
            }

            foreach (ref var chunk in _CountATwiceWithParams_Query)
            {
                ref var @intcomponentaFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentA>();
                ref var @intcomponentbFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentB>();
                foreach (var entityIndex in chunk)
                {
                    ref var @_ = ref Unsafe.Add(ref intcomponentaFirstElement, entityIndex);
                    ref var @__ = ref Unsafe.Add(ref intcomponentbFirstElement, entityIndex);
                    CountATwiceWithParams(ref @count1, in @_, ref @count2, in @__);
                }
            }
        }
    }
}