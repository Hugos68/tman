using System;
using System.Collections.Generic;
using GBX.NET;
using Tman.Core.Inventory;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Internal
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
                    var defaultOffset = Waypoint is Waypoint.Start ? new Vec3(0, 0, MapConstants.ItemStartZOffsetZ) : Vec3.Zero;
                    map.PlaceRelative(placement, EffectItem(placement.ItemModel!, gate), defaultOffset + offset);
                }
            }
        }

        private static Item EffectItem(Item waypoint, Block gate)
        {
            var effect = gate.Id["GateSpecial".Length..];
            var id = waypoint.Id
                .Replace("Start", "Special", StringComparison.Ordinal)
                .Replace("Checkpoint", "Special", StringComparison.Ordinal)
                .Replace("Finish", "Special", StringComparison.Ordinal)
                .Replace("Left", "", StringComparison.Ordinal)
                .Replace("Center", "", StringComparison.Ordinal)
                .Replace("Right", "", StringComparison.Ordinal)
                .Replace("v2", "", StringComparison.Ordinal)
                + effect;
            return Items.FromId(id) ?? throw new KeyNotFoundException($"No {effect} gate for {waypoint.Id}: {id}");
        }
    }
}
