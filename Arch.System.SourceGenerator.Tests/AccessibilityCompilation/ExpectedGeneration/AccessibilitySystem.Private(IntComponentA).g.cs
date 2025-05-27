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
        private QueryDescription Private_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private World? _Private_Initialized;
        private Query? _Private_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PrivateQuery(World world)
        {
            if (!ReferenceEquals(_Private_Initialized, world))
            {
                _Private_Query = world.Query(in Private_QueryDescription);
                _Private_Initialized = world;
            }

            foreach (ref var chunk in _Private_Query!)
            {
                ref var @intcomponentaFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentA>();
                foreach (var entityIndex in chunk)
                {
                    ref var @_ = ref Unsafe.Add(ref intcomponentaFirstElement, entityIndex);
                    Private(@_);
                }
            }
        }
    }
}
