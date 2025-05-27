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
        private static QueryDescription CountAWithParamsMiddle_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA), typeof(global::Arch.System.SourceGenerator.Tests.IntComponentB)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private static World? _CountAWithParamsMiddle_Initialized;
        private static Query? _CountAWithParamsMiddle_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CountAWithParamsMiddleQuery(World world, ref int @count)
        {
            if (!ReferenceEquals(_CountAWithParamsMiddle_Initialized, world))
            {
                _CountAWithParamsMiddle_Query = world.Query(in CountAWithParamsMiddle_QueryDescription);
                _CountAWithParamsMiddle_Initialized = world;
            }

            foreach (ref var chunk in _CountAWithParamsMiddle_Query)
            {
                ref var @intcomponentaFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentA>();
                ref var @intcomponentbFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentB>();
                foreach (var entityIndex in chunk)
                {
                    ref var @_ = ref Unsafe.Add(ref intcomponentaFirstElement, entityIndex);
                    ref var @__ = ref Unsafe.Add(ref intcomponentbFirstElement, entityIndex);
                    CountAWithParamsMiddle(in @_, ref @count, in @__);
                }
            }
        }
    }
}