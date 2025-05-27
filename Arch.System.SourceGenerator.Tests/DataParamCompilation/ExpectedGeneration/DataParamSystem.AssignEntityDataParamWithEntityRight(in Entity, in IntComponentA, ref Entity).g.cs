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
        private static QueryDescription AssignEntityDataParamWithEntityRight_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private static World? _AssignEntityDataParamWithEntityRight_Initialized;
        private static Query? _AssignEntityDataParamWithEntityRight_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AssignEntityDataParamWithEntityRightQuery(World world, ref Arch.Core.Entity @outentity)
        {
            if (!ReferenceEquals(_AssignEntityDataParamWithEntityRight_Initialized, world))
            {
                _AssignEntityDataParamWithEntityRight_Query = world.Query(in AssignEntityDataParamWithEntityRight_QueryDescription);
                _AssignEntityDataParamWithEntityRight_Initialized = world;
            }

            foreach (ref var chunk in _AssignEntityDataParamWithEntityRight_Query)
            {
                ref var entityFirstElement = ref chunk.Entity(0);
                ref var @intcomponentaFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentA>();
                foreach (var entityIndex in chunk)
                {
                    ref readonly var e = ref Unsafe.Add(ref entityFirstElement, entityIndex);
                    ref var @a = ref Unsafe.Add(ref intcomponentaFirstElement, entityIndex);
                    AssignEntityDataParamWithEntityRight(in @e, in @a, ref @outentity);
                }
            }
        }
    }
}