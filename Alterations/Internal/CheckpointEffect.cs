using GBX.NET;
using tman.Inventory;

namespace tman.Alterations.Internal
{
    public class CheckpointEffect(Block gate, Vec3 offset) : WaypointEffect(gate, offset)
    {
        protected override Waypoint Waypoint => Waypoint.Checkpoint;
    }
}
