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
        private QueryDescription Default_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private World? _Default_Initialized;
        private Query? _Default_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DefaultQuery(World world)
        {
            if (!ReferenceEquals(_Default_Initialized, world))
            {
                _Default_Query = world.Query(in Default_QueryDescription);
                _Default_Initialized = world;
            }

            foreach (ref var chunk in _Default_Query!)
            {
                ref var @intcomponentaFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentA>();
                foreach (var entityIndex in chunk)
                {
                    ref var @_ = ref Unsafe.Add(ref intcomponentaFirstElement, entityIndex);
                    Default(@_);
                }
            }
        }
    }
}
