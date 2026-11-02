using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Tman.Core.Catalog;
using Tman.Core.Maps;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Alterations.Steps
{
    internal abstract class BlockReplacement<TModel>(Func<Block, TModel?> replacement) : Alteration where TModel : class
    {
        protected sealed override void Run(Map map, AlterationContext context)
        {
            var placements = map.Placements.OnlyBlocks().ToList();
            var kept = new Dictionary<string, int>(StringComparer.Ordinal);
            var replaced = 0;
            foreach (var placement in placements)
            {
                if (placement.BlockModel is not { } model)
                {
                    context.Logger.LogWarning("{Block} at {Cell} is not in the catalog", placement.Id, placement.Cell);
                }
                else if (replacement(model) is { } target)
                {
                    Replace(map, context, placement, target);
                    context.Logger.LogDebug("{Block} at {Cell} replaced", model.Id, placement.Cell);
                    replaced++;
                }
                else
                {
                    kept[model.Id] = kept.GetValueOrDefault(model.Id) + 1;
                }
            }
            context.Logger.LogInformation("{Step}: {Replaced} of {Blocks} blocks replaced", GetType().Name, replaced, placements.Count);
            foreach (var (id, count) in kept.OrderByDescending(pair => pair.Value).ThenBy(pair => pair.Key, StringComparer.Ordinal))
            {
                context.Logger.LogDebug("Kept {Count} × {Block}", count, id);
            }
        }

        protected abstract void Replace(Map map, AlterationContext context, Placement placement, TModel target);
    }
}
