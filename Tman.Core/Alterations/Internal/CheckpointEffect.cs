using GBX.NET;
using Tman.Core.Inventory;

namespace Tman.Core.Alterations.Internal
{
    public class CheckpointEffect(Block gate, Vec3 offset) : WaypointEffect(gate, offset)
    {
        protected override Waypoint Waypoint => Waypoint.Checkpoint;
    }
}
