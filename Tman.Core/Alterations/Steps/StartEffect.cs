using GBX.NET;
using Tman.Core.Inventory;

namespace Tman.Core.Alterations.Steps
{
    public class StartEffect(Block gate, Vec3 offset) : WaypointEffect(gate, offset)
    {
        protected override Waypoint Waypoint => Waypoint.Start;
    }
}
