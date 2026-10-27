using GBX.NET;
using tman.Core;

namespace tman.Alterations.Internal
{
    public class CheckpointEffect(Block gate, Vec3 offset) : WaypointEffect(gate, offset)
    {
        protected override Waypoint Waypoint => Waypoint.Checkpoint;
    }
}
