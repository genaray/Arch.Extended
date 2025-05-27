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
        private static QueryDescription CountANullable_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private static World? _CountANullable_Initialized;
        private static Query? _CountANullable_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CountANullableQuery(World world, ref int? @count)
        {
            if (!ReferenceEquals(_CountANullable_Initialized, world))
            {
                _CountANullable_Query = world.Query(in CountANullable_QueryDescription);
                _CountANullable_Initialized = world;
            }

            foreach (ref var chunk in _CountANullable_Query!)
            {
                foreach (var entityIndex in chunk)
                {
                    CountANullable(ref @count);
                }
            }
        }
    }
}
