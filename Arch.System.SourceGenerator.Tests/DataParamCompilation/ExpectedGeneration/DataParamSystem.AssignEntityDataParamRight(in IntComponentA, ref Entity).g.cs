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
        private QueryDescription AssignEntityDataParamRight_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private World? _AssignEntityDataParamRight_Initialized;
        private Query? _AssignEntityDataParamRight_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AssignEntityDataParamRightQuery(World world, ref Arch.Core.Entity @outentity)
        {
            if (!ReferenceEquals(_AssignEntityDataParamRight_Initialized, world))
            {
                _AssignEntityDataParamRight_Query = world.Query(in AssignEntityDataParamRight_QueryDescription);
                _AssignEntityDataParamRight_Initialized = world;
            }

            foreach (ref var chunk in _AssignEntityDataParamRight_Query!)
            {
                ref var @intcomponentaFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentA>();
                foreach (var entityIndex in chunk)
                {
                    ref var @a = ref Unsafe.Add(ref intcomponentaFirstElement, entityIndex);
                    AssignEntityDataParamRight(in @a, ref @outentity);
                }
            }
        }
    }
}