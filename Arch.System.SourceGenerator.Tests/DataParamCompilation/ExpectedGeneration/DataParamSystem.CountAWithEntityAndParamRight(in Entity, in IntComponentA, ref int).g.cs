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
        private static QueryDescription CountAWithEntityAndParamRight_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private static World? _CountAWithEntityAndParamRight_Initialized;
        private static Query? _CountAWithEntityAndParamRight_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CountAWithEntityAndParamRightQuery(World world, ref int @count)
        {
            if (!ReferenceEquals(_CountAWithEntityAndParamRight_Initialized, world))
            {
                _CountAWithEntityAndParamRight_Query = world.Query(in CountAWithEntityAndParamRight_QueryDescription);
                _CountAWithEntityAndParamRight_Initialized = world;
            }

            foreach (ref var chunk in _CountAWithEntityAndParamRight_Query)
            {
                ref var entityFirstElement = ref chunk.Entity(0);
                ref var @intcomponentaFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentA>();
                foreach (var entityIndex in chunk)
                {
                    ref readonly var e = ref Unsafe.Add(ref entityFirstElement, entityIndex);
                    ref var @a = ref Unsafe.Add(ref intcomponentaFirstElement, entityIndex);
                    CountAWithEntityAndParamRight(in @e, in @a, ref @count);
                }
            }
        }
    }
}