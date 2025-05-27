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
    partial class BasicSystem
    {
        private QueryDescription Basic_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private World? _Basic_Initialized;
        private Query? _Basic_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BasicQuery(World world)
        {
            if (!ReferenceEquals(_Basic_Initialized, world))
            {
                _Basic_Query = world.Query(in Basic_QueryDescription);
                _Basic_Initialized = world;
            }

            foreach (ref var chunk in _Basic_Query)
            {
                ref var @intcomponentaFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentA>();
                foreach (var entityIndex in chunk)
                {
                    ref var @_ = ref Unsafe.Add(ref intcomponentaFirstElement, entityIndex);
                    Basic(@_);
                }
            }
        }
    }
}