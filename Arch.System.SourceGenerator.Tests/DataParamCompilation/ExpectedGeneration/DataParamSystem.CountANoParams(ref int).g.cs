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
        private static QueryDescription CountANoParams_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private static World? _CountANoParams_Initialized;
        private static Query? _CountANoParams_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CountANoParamsQuery(World world, ref int @count)
        {
            if (!ReferenceEquals(_CountANoParams_Initialized, world))
            {
                _CountANoParams_Query = world.Query(in CountANoParams_QueryDescription);
                _CountANoParams_Initialized = world;
            }

            foreach (ref var chunk in _CountANoParams_Query)
            {
                foreach (var entityIndex in chunk)
                {
                    CountANoParams(ref @count);
                }
            }
        }
    }
}