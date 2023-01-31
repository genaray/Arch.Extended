using Arch.Core;

namespace Arch.System.SourceGenerator.Tests
{
    public partial class MultipleMethod : BaseSystem<World, CommonArgs>
    {
        public int IsWork = 0;
        public MultipleMethod(World world) : base(world)
        {
        }

        [Query]
        [All(typeof(TestComponent))]
        public void DoJob0(ref TestComponent test)
        {
            IsWork++;
        }
        
        [Query]
        [Any(typeof(TestComponent))]
        public void DoJob1(ref TestComponent x)
        {
            IsWork++;
        }
        
        [Query]
        [None(typeof(TestComponent))]
        public void DoJob2(ref TestComponent x)
        {
            IsWork++;
        }
        
        [Query]
        [Exclusive(typeof(TestComponent),typeof(PositionComponent))]
        public void DoJob3(ref TestComponent x)
        {
            IsWork++;
        }
        
        [Query]
        public void DoJob4()
        {
            IsWork++;
        }
    }
}