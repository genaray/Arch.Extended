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
        private static QueryDescription CountAWithParamsLeft_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private static World? _CountAWithParamsLeft_Initialized;
        private static Query? _CountAWithParamsLeft_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CountAWithParamsLeftQuery(World world, ref int @count)
        {
            if (!ReferenceEquals(_CountAWithParamsLeft_Initialized, world))
            {
                _CountAWithParamsLeft_Query = world.Query(in CountAWithParamsLeft_QueryDescription);
                _CountAWithParamsLeft_Initialized = world;
            }

            foreach (ref var chunk in _CountAWithParamsLeft_Query)
            {
                ref var @intcomponentaFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentA>();
                foreach (var entityIndex in chunk)
                {
                    ref var @_ = ref Unsafe.Add(ref intcomponentaFirstElement, entityIndex);
                    CountAWithParamsLeft(ref @count, in @_);
                }
            }
        }
    }
}