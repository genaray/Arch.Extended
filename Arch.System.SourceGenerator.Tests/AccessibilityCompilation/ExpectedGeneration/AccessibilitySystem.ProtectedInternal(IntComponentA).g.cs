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
        private QueryDescription ProtectedInternal_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private World? _ProtectedInternal_Initialized;
        private Query? _ProtectedInternal_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected internal void ProtectedInternalQuery(World world)
        {
            if (!ReferenceEquals(_ProtectedInternal_Initialized, world))
            {
                _ProtectedInternal_Query = world.Query(in ProtectedInternal_QueryDescription);
                _ProtectedInternal_Initialized = world;
            }

            foreach (ref var chunk in _ProtectedInternal_Query!)
            {
                ref var @intcomponentaFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentA>();
                foreach (var entityIndex in chunk)
                {
                    ref var @_ = ref Unsafe.Add(ref intcomponentaFirstElement, entityIndex);
                    ProtectedInternal(@_);
                }
            }
        }
    }
}
