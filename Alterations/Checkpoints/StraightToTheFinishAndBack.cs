using System.Collections.Generic;
using tman.Alterations.Internal;
using tman.Inventory;
using tman.Maps;

namespace tman.Alterations.Checkpoints
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
            foreach (var finish in map.Waypoints(Waypoint.Finish))
            {
                if (finish.BlockModel == Blocks.GateExpandableFinish)
                {
                    map.Embed(Custom.CheckpointExpandable);
                    map.PlaceRelative(finish, Custom.CheckpointExpandable);
                    map.Remove(finish);
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
    }
}
