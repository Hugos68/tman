using System;
using System.Collections.Generic;
using System.Linq;
using GBX.NET;
using Tman.Core.Alterations.Steps.Waypoints;
using Tman.Core.Catalog;
using Tman.Core.Maps;
using Tman.Core.Maps.Geometry;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Alterations.Finishes
{
    public class StraightToTheFinishAndBack : StandardAlteration
    {
        protected override string DisplayName => "Straight to the Finish and Back";

        protected override IEnumerable<Alteration> Steps =>
        [
            new CheckpointsCleaned()
        ];

        protected override void Run(Map map)
        {
            var finishes = map.Waypoints(Waypoint.Finish);
            var segments = new HashSet<string>();
            foreach (var finish in finishes)
            {
                if (finish.BlockModel == Blocks.GateExpandableFinish)
                {
                    segments.Add(Key(map, finish, Vec3.Zero));
                }
            }
            foreach (var finish in finishes)
            {
                if (finish.BlockModel == Blocks.GateExpandableFinish)
                {
                    ReplaceSegment(map, finish, segments);
                }
                else if (finish.BlockModel == Blocks.GateFinish)
                {
                    ReplaceRing(map, finish);
                }
                else
                {
                    map.Replace(finish, Waypoint.Checkpoint);
                }
            }
            foreach (var start in map.Waypoints(Waypoint.Start))
            {
                map.PlaceLike(start, Waypoint.Finish);
            }
            map.Link(map.Waypoints(Waypoint.Checkpoint));
        }

        private static void ReplaceRing(Map map, Placement ring)
        {
            var pillar = ring.IsGround ? 0 : ring.Variant / BlockFlags.VariantsPerAdditionalVariant;
            Place(map, ring, pillar == 1 ? Custom.CheckpointRingPillar : pillar == 2 ? Custom.CheckpointRingInPillar : Custom.CheckpointRing);
            map.Remove(ring);
        }

        private static void ReplaceSegment(Map map, Placement segment, HashSet<string> segments)
        {
            var inPillar = segment.Variant / BlockFlags.VariantsPerAdditionalVariant;
            Place(map, segment, Custom.CheckpointExpandable);
            if (!segments.Contains(Key(map, segment, new Vec3(0, -MapConstants.CellHeight, 0))))
            {
                Place(map, segment, segment.IsGround ? Custom.CheckpointExpandableFoot : inPillar == 1 ? Custom.CheckpointExpandableBottom : Custom.CheckpointExpandableBottomInPillar);
            }
            if (!segments.Contains(Key(map, segment, new Vec3(0, MapConstants.CellHeight, 0))))
            {
                Place(map, segment, inPillar == (segment.IsGround ? 1 : 2) ? Custom.CheckpointExpandableTopInPillar : Custom.CheckpointExpandableTop);
            }
            if (!segments.Contains(Key(map, segment, new Vec3(-MapConstants.CellSize, 0, 0))))
            {
                Place(map, segment, Custom.CheckpointExpandableRight);
            }
            if (!segments.Contains(Key(map, segment, new Vec3(MapConstants.CellSize, 0, 0))))
            {
                Place(map, segment, Custom.CheckpointExpandableLeft);
            }
            map.Remove(segment);
        }

        private static string Key(Map map, Placement segment, Vec3 offset)
        {
            var pose = segment.Pose(map);
            pose.Move(offset);
            var origin = pose.Coords;
            pose.Move(new Vec3(1, 0, 0));
            var x = pose.Coords - origin;
            pose.Move(new Vec3(-1, 1, 0));
            var y = pose.Coords - origin;
            return string.Join(" ", new[] { origin.X, origin.Y, origin.Z }.Select(v => Math.Round(v, 1)).Concat(new[] { x.X, x.Y, x.Z, y.X, y.Y, y.Z }.Select(v => Math.Round(v, 2))));
        }

        private static void Place(Map map, Placement segment, Item item)
        {
            var skinned = item.For(map.Collection);
            map.Embed(skinned);
            map.PlaceRelative(segment, skinned);
        }
    }
}
