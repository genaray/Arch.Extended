using System.Diagnostics;
using Arch.Core;

namespace Arch.System.SourceGenerator.Tests
{
    public partial class SingleMethod : BaseSystem<World, CommonArgs>
    {
        public int IsWork = 0;
        public SingleMethod(World world) : base(world)
        {
        }

        [Query]
        [All(typeof(TestComponent),typeof(PositionComponent))]
        public void DoJob([Data] CommonArgs test,ref PositionComponent positionComponent)
        {
            if(positionComponent.x == 1 && positionComponent.y == 1 && positionComponent.z == 1)
                IsWork++;
        }
    }
}