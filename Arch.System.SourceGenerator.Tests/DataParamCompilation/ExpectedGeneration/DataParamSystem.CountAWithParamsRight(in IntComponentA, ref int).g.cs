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
        private static QueryDescription CountAWithParamsRight_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private static World? _CountAWithParamsRight_Initialized;
        private static Query? _CountAWithParamsRight_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CountAWithParamsRightQuery(World world, ref int @count)
        {
            if (!ReferenceEquals(_CountAWithParamsRight_Initialized, world))
            {
                _CountAWithParamsRight_Query = world.Query(in CountAWithParamsRight_QueryDescription);
                _CountAWithParamsRight_Initialized = world;
            }

            foreach (ref var chunk in _CountAWithParamsRight_Query)
            {
                ref var @intcomponentaFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentA>();
                foreach (var entityIndex in chunk)
                {
                    ref var @_ = ref Unsafe.Add(ref intcomponentaFirstElement, entityIndex);
                    CountAWithParamsRight(in @_, ref @count);
                }
            }
        }
    }
}