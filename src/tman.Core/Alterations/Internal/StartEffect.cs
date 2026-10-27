using GBX.NET;
using tman.Core;

namespace tman.Alterations.Internal
{
    public class StartEffect(Block gate, Vec3 offset) : WaypointEffect(gate, offset)
    {
        protected override Waypoint Waypoint => Waypoint.Start;
    }
}
