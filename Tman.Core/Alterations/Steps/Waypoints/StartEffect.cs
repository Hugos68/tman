using GBX.NET;
using Tman.Core.Catalog;

namespace Tman.Core.Alterations.Steps.Waypoints
{
    internal sealed class StartEffect(Block gate, Vec3 offset) : WaypointEffect(gate, offset)
    {
        protected override Waypoint Waypoint => Waypoint.Start;
    }
}
