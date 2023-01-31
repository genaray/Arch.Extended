using Arch.Core;

namespace Arch.System.SourceGenerator.Tests
{
    public partial class OnlyDataParameter : BaseSystem<World,CommonArgs>
    {
        public int IsWork = 0;
        public OnlyDataParameter(World world) : base(world)
        {
        }

        [Query]
        public void DoJob([Data] CommonArgs args)
        {
            if (args.Value == 1)
                IsWork++;
        }
    }
}