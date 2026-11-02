using System;
using System.Linq;
using Tman.Core.Catalog;
using Tman.Core.Maps;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Alterations.Steps
{
    internal abstract class BlockReplacement<TModel>(Func<Block, TModel?> replacement) : Alteration where TModel : class
    {
        protected sealed override void Run(Map map, AlterationContext context)
        {
            foreach (var placement in map.Placements.OnlyBlocks().ToList())
            {
                if (placement.BlockModel is { } model && replacement(model) is { } target)
                {
                    Replace(map, context, placement, target);
                }
            }
        }

        protected abstract void Replace(Map map, AlterationContext context, Placement placement, TModel target);
    }
}
