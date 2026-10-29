using GBX.NET;
using Tman.Inventory;

namespace Tman.Alterations.Internal
{
    public class CheckpointEffect(Block gate, Vec3 offset) : WaypointEffect(gate, offset)
    {
        protected override Waypoint Waypoint => Waypoint.Checkpoint;
    }
}
