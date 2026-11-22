using Tman.Core.Maps;

namespace Tman.Core.Alterations.Steps
{
    internal sealed class BakedClipsRemoved : Alteration
    {
        protected override void Run(Map map, AlterationContext context) => map.RemoveBakedClips();
    }
}
