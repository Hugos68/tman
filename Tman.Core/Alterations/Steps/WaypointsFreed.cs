using System.Linq;
using GBX.NET;
using Microsoft.Extensions.Logging;
using Tman.Core.Maps;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Alterations.Steps
{
    internal sealed class WaypointsFreed : Alteration
    {
        protected override void Run(Map map, AlterationContext context)
        {
            var waypoints = map.Placements.OnlyBlocks().Where(placement => placement.IsWaypoint && !placement.IsFree).ToList();
            var covered = waypoints.SelectMany(placement => placement.Cells().Select(cell => new Int3(cell.X, placement.Cell.Y, cell.Z))).ToHashSet();
            var terrain = map.Placements.OnlyBlocks().Where(placement => !placement.IsWaypoint && !placement.IsFree && covered.Contains(placement.Cell)).ToList();
            foreach (var placement in terrain)
            {
                map.Remove(placement);
            }
            foreach (var placement in waypoints)
            {
                var ground = placement.Block!.IsGround;
                map.Free(placement);
                placement.Block.IsGround = ground;
            }
            context.Logger.LogInformation("{Step}: {Freed} waypoint blocks freed, {Terrain} terrain blocks under them removed", GetType().Name, waypoints.Count, terrain.Count);
        }
    }
}
