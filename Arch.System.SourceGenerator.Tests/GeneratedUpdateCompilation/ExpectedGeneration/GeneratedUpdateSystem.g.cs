using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;

namespace Arch.System.SourceGenerator.Tests
{
    partial class GeneratedUpdateSystem
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Update(in int data)
        {
            AutoRunAQuery(World);
            AutoRunBQuery(World);
        }
    }
}