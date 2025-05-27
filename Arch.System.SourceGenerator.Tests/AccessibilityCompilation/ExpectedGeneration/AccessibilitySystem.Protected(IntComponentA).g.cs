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
        private QueryDescription Protected_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private World? _Protected_Initialized;
        private Query? _Protected_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProtectedQuery(World world)
        {
            if (!ReferenceEquals(_Protected_Initialized, world))
            {
                _Protected_Query = world.Query(in Protected_QueryDescription);
                _Protected_Initialized = world;
            }

            foreach (ref var chunk in _Protected_Query!)
            {
                ref var @intcomponentaFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentA>();
                foreach (var entityIndex in chunk)
                {
                    ref var @_ = ref Unsafe.Add(ref intcomponentaFirstElement, entityIndex);
                    Protected(@_);
                }
            }
        }
    }
}
