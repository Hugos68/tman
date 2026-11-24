using System.Linq;
using Microsoft.Extensions.Logging;
using Tman.Core.Catalog;
using Tman.Core.Maps;
using Tman.Core.Maps.Placements;
using Tman.Core.Surfaces;

namespace Tman.Core.Alterations.Steps
{
    internal sealed class WaypointsUnclipped : Alteration
    {
        protected override void Run(Map map, AlterationContext context)
        {
            var waypoints = map.Placements.OnlyBlocks().Where(placement => placement.IsWaypoint && placement.IsFree && placement.BlockModel is { Waypoint: Waypoint.Checkpoint or Waypoint.Finish } model && DeckFamily.Of(model) is not null).ToList();
            var unclipped = waypoints.Count(placement =>
            {
                if (!context.Replace(map, placement, new SurfaceBlock(Surface.Waypoint, placement.BlockModel!)))
                {
                    return false;
                }
                placement.Block!.IsGround = false;
                return true;
            });
            context.Logger.LogInformation("{Step}: {Unclipped} of {Waypoints} road and platform checkpoints and finishes are custom blocks that keep their look, trigger and respawn on a gate with no clips", GetType().Name, unclipped, waypoints.Count);
        }
    }
}
