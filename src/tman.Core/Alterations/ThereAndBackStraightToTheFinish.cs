using System;
using System.Collections.Generic;
using tman.Alterations.Internal;
using tman.Core;
using tman.Maps;
using tman.Positioning;

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
            foreach (var placement in map.Waypoints(Waypoint.Start))
            {
                if (placement.IsBlock)
                {
                    var startFinish = Inventory.Block(placement.Id.Replace("Start", "Multilap"));
                    if (startFinish != null)
                    {
                        map.Replace(placement, startFinish, DirectionOffset.None);
                    }
                }
                else
                {
                    var startFinish = Inventory.Item(placement.Id.Replace("Start", "Multilap"));
                    if (startFinish != null)
                    {
                        map.Replace(placement, startFinish);
                    }
                }
            }

            foreach (var placement in map.Waypoints(Waypoint.Finish))
            {
                if (placement.IsBlock)
                {
                    var checkpoint = Inventory.Block(placement.Id.Replace("Finish", "Checkpoint"));
                    if (checkpoint != null)
                    {
                        map.Replace(placement, checkpoint, DirectionOffset.None) ;
                    }
                }
                else
                {
                    var checkpoint = Inventory.Item(placement.Id.Replace("Finish", "Checkpoint"));
                    if (checkpoint != null)
                    {
                        map.Replace(placement, checkpoint);
                    }
                }
            }
        }
    }
}