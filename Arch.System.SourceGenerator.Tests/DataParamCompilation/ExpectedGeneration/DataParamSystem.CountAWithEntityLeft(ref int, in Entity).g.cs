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
        private static QueryDescription CountAWithEntityLeft_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private static World? _CountAWithEntityLeft_Initialized;
        private static Query? _CountAWithEntityLeft_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CountAWithEntityLeftQuery(World world, ref int @count)
        {
            if (!ReferenceEquals(_CountAWithEntityLeft_Initialized, world))
            {
                _CountAWithEntityLeft_Query = world.Query(in CountAWithEntityLeft_QueryDescription);
                _CountAWithEntityLeft_Initialized = world;
            }

            foreach (ref var chunk in _CountAWithEntityLeft_Query)
            {
                ref var entityFirstElement = ref chunk.Entity(0);
                foreach (var entityIndex in chunk)
                {
                    ref readonly var e = ref Unsafe.Add(ref entityFirstElement, entityIndex);
                    CountAWithEntityLeft(ref @count, in @e);
                }
            }
        }
    }
}