using System.Collections.Generic;
using System.Linq;
using Tman.Core.Catalog;
using Tman.Core.Maps;
using Tman.Core.Maps.Geometry;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Alterations.Steps.Waypoints
{
    internal sealed class CheckpointsCleaned : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new PlacementsRemoved(
                placement => placement.IsCheckpoint && (!placement.IsBlock || placement.BlockModel == Blocks.GateCheckpoint)
            ),
        ];

        protected override void Run(Map map, AlterationContext context)
        {
            foreach (var placement in map.Placements.OnlyBlocks().Where(placement => placement.BlockModel?.Waypoint == Waypoint.Checkpoint).ToList())
            {
                if (!Blocks.PlainOf.TryGetValue(placement.Id, out var plain))
                {
                    throw new KeyNotFoundException($"No plain block for the checkpoint '{placement.Id}'");
                }
                map.Replace(placement, plain.Block, plain.Rotation);
            }
        }
    }
}
