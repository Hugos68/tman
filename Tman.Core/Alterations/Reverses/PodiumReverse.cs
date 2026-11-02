using System.Collections.Generic;
using System.Linq;
using GBX.NET;
using Microsoft.Extensions.Logging;
using Tman.Core.Alterations.Steps.Waypoints;
using Tman.Core.Catalog;
using Tman.Core.Maps;
using Tman.Core.Maps.Geometry;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Alterations.Reverses
{
    internal sealed class PodiumReverse : StandardAlteration
    {
        private const float PodiumFront = 2.79f;

        private const float CarRear = 1.924f;

        private const float Gap = 0.05f;

        protected override string DisplayName => "Podium Reverse";

        protected override IEnumerable<Alteration> Steps => [new CheckpointsCleaned()];

        protected override void Run(Map map, AlterationContext context)
        {
            var starts = map.Waypoints(Waypoint.Start);
            var podiums = map.Of(Items.Podium);
            if (starts.Count == 0 || podiums.Count == 0)
            {
                context.Logger.LogWarning("{Starts} starts and {Podiums} podiums: the map stays as it is", starts.Count, podiums.Count);
                return;
            }
            var origin = starts[0].Pose(map).Coords;
            var podium = podiums.MinBy(candidate => (candidate.Pose(map).Coords - origin).GetSqrMagnitude())!;
            foreach (var finish in map.Waypoints(Waypoint.Finish))
            {
                if (finish.BlockModel?.As(Waypoint.Checkpoint) is { } checkpoint && Blocks.PlainOf.TryGetValue(checkpoint.Id, out var plain))
                {
                    map.Replace(finish, plain.Block, plain.Rotation);
                }
                else
                {
                    map.Remove(finish);
                }
            }
            foreach (var start in starts)
            {
                map.Replace(start, Waypoint.Finish);
            }
            map.PlaceRelative(podium, Items.GateStartCenter8m, new Vec3(0, 0, PodiumFront + CarRear + Gap - MapConstants.ItemStartZOffsetZ));
            context.Logger.LogInformation("Start at the podium at {Position}, {Starts} starts turned into finishes", podium.Pose(map).Coords, starts.Count);
        }
    }
}
