using GBX.NET;
using Tman.Core.Inventory;

namespace Tman.Core.Alterations.Steps
{
    public class CheckpointEffect(Block gate, Vec3 offset) : WaypointEffect(gate, offset)
    {
        protected override Waypoint Waypoint => Waypoint.Checkpoint;
    }
}
