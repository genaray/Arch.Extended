using Arch.Core;

namespace Arch.System.SourceGenerator.Tests
{
    public partial class EmptyQueryParameter : BaseSystem<World,CommonArgs>
    {
        public int IsWork = 0;
        public EmptyQueryParameter(World world) : base(world)
        {
        }

        [Query]
        public void DoJob()
        {
            IsWork++;
        }
    }
}