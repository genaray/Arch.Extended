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
    partial class AccessibilitySystem
    {
        private QueryDescription Internal_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private World? _Internal_Initialized;
        private Query? _Internal_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void InternalQuery(World world)
        {
            if (!ReferenceEquals(_Internal_Initialized, world))
            {
                _Internal_Query = world.Query(in Internal_QueryDescription);
                _Internal_Initialized = world;
            }

            foreach (ref var chunk in _Internal_Query!)
            {
                ref var @intcomponentaFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentA>();
                foreach (var entityIndex in chunk)
                {
                    ref var @_ = ref Unsafe.Add(ref intcomponentaFirstElement, entityIndex);
                    Internal(@_);
                }
            }
        }
    }
}
