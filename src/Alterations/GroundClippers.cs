using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src.Alterations.Internal;
using tman.src.Application;
using tman.src.Core;
using tman.src.Inventory;
using tman.src.Positioning;

namespace tman.src.Alterations
{
    public class GroundClippers : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new InventoryRemoved(InventoryUtility.IsFinish, InventoryUtility.IsFinish),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Ground Clippers")
        ];

        private static readonly Dictionary<string, (string[] Tiles, float Bed)> Floors = new()
        {
            ["RedIsland"] = (["Water", "WaterHill", "WaterShore1", "Dirt"], 2.0f),
            ["WhiteShore"] = (["Water", "WaterShore1", "Land"], 2.0f),
            ["BlueBay"] = (["Sea", "Beach", "SeaCliff13", "Sea_Land0_Land12"], 4.0f),
            ["GreenCoast"] = (["Lake", "LakeShore", "Grass"], 2.0f),
            ["Stadium2020"] = (["Grass"], 0.0f),
        };

        private const float TopUnderFloor = 0.15f;
        private const float Thickness = 1.0f;
        private const float Margin = 6 * MapConstants.CellSize;

        protected override void Run(CGameCtnChallenge map)
        {
            if (!Floors.TryGetValue(map.GetEnvironment(), out var floor))
            {
                return;
            }
            var rows = (map.Blocks ?? []).Concat(map.BakedBlocks ?? [])
                .Where(b => !b.IsFree && floor.Tiles.Contains(b.BlockModel.Id))
                .Select(b => b.Coord.Y)
                .ToList();
            if (rows.Count == 0)
            {
                return;
            }
            var top = (rows.Min() - MapUtility.GroundRow(map)) * MapConstants.CellHeight + floor.Bed - TopUnderFloor;
            var box = new FinishPlane.Box(-Margin, map.Size.X * MapConstants.CellSize + Margin, -Margin, map.Size.Z * MapConstants.CellSize + Margin, top - Thickness, top);
            FinishPlane.Place(
                map,
                Path.Combine(ApplicationConstants.ProjectDirectory, "data", "items", "FinishPlaneTemplate.Item.Gbx"),
                $"GroundClippers_{map.MapUid}.Item.Gbx",
                [box], false);
        }
    }
}
