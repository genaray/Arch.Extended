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
        private static QueryDescription AssignEntityDataParamWithEntityLeft_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private static World? _AssignEntityDataParamWithEntityLeft_Initialized;
        private static Query? _AssignEntityDataParamWithEntityLeft_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AssignEntityDataParamWithEntityLeftQuery(World world, ref Arch.Core.Entity @outentity)
        {
            if (!ReferenceEquals(_AssignEntityDataParamWithEntityLeft_Initialized, world))
            {
                _AssignEntityDataParamWithEntityLeft_Query = world.Query(in AssignEntityDataParamWithEntityLeft_QueryDescription);
                _AssignEntityDataParamWithEntityLeft_Initialized = world;
            }

            foreach (ref var chunk in _AssignEntityDataParamWithEntityLeft_Query!)
            {
                ref var entityFirstElement = ref chunk.Entity(0);
                ref var @intcomponentaFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentA>();
                foreach (var entityIndex in chunk)
                {
                    ref readonly var e = ref Unsafe.Add(ref entityFirstElement, entityIndex);
                    ref var @a = ref Unsafe.Add(ref intcomponentaFirstElement, entityIndex);
                    AssignEntityDataParamWithEntityLeft(ref @outentity, in @e, in @a);
                }
            }
        }
    }
}