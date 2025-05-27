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
        private QueryDescription AssignEntityDataParamLeft_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private World? _AssignEntityDataParamLeft_Initialized;
        private Query? _AssignEntityDataParamLeft_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AssignEntityDataParamLeftQuery(World world, ref Arch.Core.Entity @outentity)
        {
            if (!ReferenceEquals(_AssignEntityDataParamLeft_Initialized, world))
            {
                _AssignEntityDataParamLeft_Query = world.Query(in AssignEntityDataParamLeft_QueryDescription);
                _AssignEntityDataParamLeft_Initialized = world;
            }

            foreach (ref var chunk in _AssignEntityDataParamLeft_Query!)
            {
                ref var @intcomponentaFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentA>();
                foreach (var entityIndex in chunk)
                {
                    ref var @a = ref Unsafe.Add(ref intcomponentaFirstElement, entityIndex);
                    AssignEntityDataParamLeft(ref @outentity, in @a);
                }
            }
        }
    }
}