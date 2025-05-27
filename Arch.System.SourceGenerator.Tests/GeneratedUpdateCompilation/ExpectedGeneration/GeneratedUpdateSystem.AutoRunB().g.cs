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
    partial class GeneratedUpdateSystem
    {
        private QueryDescription AutoRunB_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private World? _AutoRunB_Initialized;
        private Query? _AutoRunB_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AutoRunBQuery(World world)
        {
            if (!ReferenceEquals(_AutoRunB_Initialized, world))
            {
                _AutoRunB_Query = world.Query(in AutoRunB_QueryDescription);
                _AutoRunB_Initialized = world;
            }

            foreach (ref var chunk in _AutoRunB_Query)
            {
                foreach (var entityIndex in chunk)
                {
                    AutoRunB();
                }
            }
        }
    }
}