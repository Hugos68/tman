using System.Collections.Generic;
using tman.Alterations.Internal;
using tman.Core;
using tman.Maps;

namespace tman.Alterations
{
    public class ThereAndBackStraightToTheFinish : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " There and Back Straight to the Finish")
        ];

        protected override void Run(Map map)
        {
            var finishes = map.Waypoints(Waypoint.Finish);
            foreach (var start in map.Waypoints(Waypoint.Start))
            {
                map.PlaceLike(start, Waypoint.Finish);
            }
            foreach (var finish in finishes)
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
        }
    }
}
