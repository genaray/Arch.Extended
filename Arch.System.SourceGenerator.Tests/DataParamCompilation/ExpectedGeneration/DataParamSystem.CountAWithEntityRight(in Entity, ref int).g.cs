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
        private static QueryDescription CountAWithEntityRight_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private static World? _CountAWithEntityRight_Initialized;
        private static Query? _CountAWithEntityRight_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CountAWithEntityRightQuery(World world, ref int @count)
        {
            if (!ReferenceEquals(_CountAWithEntityRight_Initialized, world))
            {
                _CountAWithEntityRight_Query = world.Query(in CountAWithEntityRight_QueryDescription);
                _CountAWithEntityRight_Initialized = world;
            }

            foreach (ref var chunk in _CountAWithEntityRight_Query)
            {
                ref var entityFirstElement = ref chunk.Entity(0);
                foreach (var entityIndex in chunk)
                {
                    ref readonly var e = ref Unsafe.Add(ref entityFirstElement, entityIndex);
                    CountAWithEntityRight(in @e, ref @count);
                }
            }
        }
    }
}