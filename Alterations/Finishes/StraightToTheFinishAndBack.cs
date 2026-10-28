using System.Collections.Generic;
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
            var segments = new HashSet<(Int3 Cell, Direction Direction)>();
            foreach (var finish in finishes)
            {
                if (finish.BlockModel == Blocks.GateExpandableFinish && !finish.IsFree)
                {
                    segments.Add((finish.Cell, finish.Direction));
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

        private static void ReplaceSegment(Map map, Placement segment, HashSet<(Int3, Direction)> segments)
        {
            var cell = segment.Cell;
            var direction = segment.Direction;
            var (dx, dz) = direction switch
            {
                Direction.East => (0, 1),
                Direction.South => (-1, 0),
                Direction.West => (0, -1),
                _ => (1, 0),
            };
            Place(map, segment, Custom.CheckpointExpandable);
            if (segment.IsFree || !segments.Contains((cell + new Int3(0, -1, 0), direction)))
            {
                Place(map, segment, segment.IsGround ? Custom.CheckpointExpandableFoot : Custom.CheckpointExpandableBottom);
            }
            if (segment.IsFree || !segments.Contains((cell + new Int3(0, 1, 0), direction)))
            {
                Place(map, segment, Custom.CheckpointExpandableTop);
            }
            if (segment.IsFree || !segments.Contains((cell + new Int3(-dx, 0, -dz), direction)))
            {
                Place(map, segment, Custom.CheckpointExpandableRight);
            }
            if (segment.IsFree || !segments.Contains((cell + new Int3(dx, 0, dz), direction)))
            {
                Place(map, segment, Custom.CheckpointExpandableLeft);
            }
            map.Remove(segment);
        }

        private static void Place(Map map, Placement segment, Item item)
        {
            map.Embed(item);
            map.PlaceRelative(segment, item);
        }
    }
}
