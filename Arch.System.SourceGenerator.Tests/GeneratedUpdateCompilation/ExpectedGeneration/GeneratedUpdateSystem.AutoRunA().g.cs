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
        private QueryDescription AutoRunA_QueryDescription = new QueryDescription(all: new Signature(typeof(global::Arch.System.SourceGenerator.Tests.IntComponentA)), any: Signature.Null, none: Signature.Null, exclusive: Signature.Null);
        private World? _AutoRunA_Initialized;
        private Query? _AutoRunA_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AutoRunAQuery(World world)
        {
            if (!ReferenceEquals(_AutoRunA_Initialized, world))
            {
                _AutoRunA_Query = world.Query(in AutoRunA_QueryDescription);
                _AutoRunA_Initialized = world;
            }

            foreach (ref var chunk in _AutoRunA_Query)
            {
                foreach (var entityIndex in chunk)
                {
                    AutoRunA();
                }
            }
        }
    }
}