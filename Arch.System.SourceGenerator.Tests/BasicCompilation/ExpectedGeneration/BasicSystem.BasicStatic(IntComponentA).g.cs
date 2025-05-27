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
        private static QueryDescription BasicStatic_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private static World? _BasicStatic_Initialized;
        private static Query? _BasicStatic_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BasicStaticQuery(World world)
        {
            if (!ReferenceEquals(_BasicStatic_Initialized, world))
            {
                _BasicStatic_Query = world.Query(in BasicStatic_QueryDescription);
                _BasicStatic_Initialized = world;
            }

            foreach (ref var chunk in _BasicStatic_Query)
            {
                ref var @intcomponentaFirstElement = ref chunk.GetFirst<global::Arch.System.SourceGenerator.Tests.IntComponentA>();
                foreach (var entityIndex in chunk)
                {
                    ref var @_ = ref Unsafe.Add(ref intcomponentaFirstElement, entityIndex);
                    BasicStatic(@_);
                }
            }
        }
    }
}