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
        private QueryDescription Public_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private World? _Public_Initialized;
        private Query? _Public_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PublicQuery(World world)
        {
            if (!ReferenceEquals(_Public_Initialized, world))
            {
                _Public_Query = world.Query(in Public_QueryDescription);
                _Public_Initialized = world;
            }

            foreach (ref var chunk in _Public_Query!)
            {
                ref var @intcomponentaFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentA>();
                foreach (var entityIndex in chunk)
                {
                    ref var @_ = ref Unsafe.Add(ref intcomponentaFirstElement, entityIndex);
                    Public(@_);
                }
            }
        }
    }
}