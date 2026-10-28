using System;
using System.Collections.Generic;
using System.Linq;
using GBX.NET;
using tman.Alterations.Internal;
using tman.Inventory;
using tman.Maps;

namespace tman.Alterations.Finishes
{
    public class StraightToTheFinishAndBack : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Straight to the Finish and Back")
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

        private static void ReplaceSegment(Map map, Placement segment, HashSet<string> segments)
        {
            Place(map, segment, Custom.CheckpointExpandable);
            if (!segments.Contains(Key(map, segment, new Vec3(0, -MapConstants.CellHeight, 0))))
            {
                Place(map, segment, segment.IsGround ? Custom.CheckpointExpandableFoot : Custom.CheckpointExpandableBottom);
            }
            if (!segments.Contains(Key(map, segment, new Vec3(0, MapConstants.CellHeight, 0))))
            {
                Place(map, segment, Custom.CheckpointExpandableTop);
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
            map.Embed(item);
            map.PlaceRelative(segment, item);
        }
    }
}