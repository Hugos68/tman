using GBX.NET;
using tman.Core;
using tman.Maps;

namespace tman.Alterations.Internal
{
    public abstract class WaypointEffect(Block gate, Vec3 offset) : Alteration
    {
        protected abstract Waypoint Waypoint { get; }

        protected override void Run(Map map)
        {
            foreach (var placement in map.Waypoints(Waypoint))
            {
                if (placement.IsBlock)
                {
                    var offsetY = placement.BlockModel == Blocks.GateCheckpoint ? 0 : -MapConstants.CellHeight * 1.75f;
                    map.PlaceRelative(placement, gate, offset + new Vec3(0, offsetY, 0));
                }
                else
                {
                    var defaultOffset = Waypoint is Core.Waypoint.Start ? new Vec3(0, 0, MapConstants.ItemStartZOffsetZ) : Vec3.Zero;
                    map.PlaceRelative(placement, Inventory.EffectItem(placement.ItemModel!, gate), defaultOffset + offset);
                }
            }
        }
    }
}
