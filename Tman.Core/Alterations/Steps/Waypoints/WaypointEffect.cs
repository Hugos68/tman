using System;
using System.Collections.Generic;
using GBX.NET;
using Tman.Core.Catalog;
using Tman.Core.Maps;
using Tman.Core.Maps.Geometry;

namespace Tman.Core.Alterations.Steps.Waypoints
{
    internal abstract class WaypointEffect(Block gate, Vec3 offset) : Alteration
    {
        protected abstract Waypoint Waypoint { get; }

        protected override void Run(Map map, AlterationContext context)
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
                    var defaultOffset = Waypoint is Waypoint.Start ? new Vec3(0, 0, MapConstants.ItemStartZOffsetZ) : Vec3.Zero;
                    map.PlaceRelative(placement, EffectItem(placement.ItemModel!, gate), defaultOffset + offset);
                }
            }
        }

        private static Item EffectItem(Item waypoint, Block gate) =>
            Items.SpecialGates.GetValueOrDefault((waypoint.Id, gate.Id)) ?? throw new KeyNotFoundException($"No {gate.Id} item for {waypoint.Id}");
    }
}
