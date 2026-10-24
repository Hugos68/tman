using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.GameData;
using GBX.NET.Engines.Plug;
using tman.src.Alterations.Internal;
using tman.src.Application;
using tman.src.Inventory;
using tman.src.Positioning;

namespace tman.src.Alterations
{
    public class GroundClippers : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new WithoutValidation(),
            new WithoutInventory(InventoryUtility.IsFinish, InventoryUtility.IsFinish),
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

            var y = (rows.Min() - MapUtility.GroundRow(map)) * GridConstants.VerticalBlockSize + floor.Bed;
            var top = y - TopUnderFloor;
            var box = new FinishPlane.Box(0, map.Size.X * GridConstants.BlockSize, 0, map.Size.Z * GridConstants.BlockSize, top - Thickness, top, top - 0.02f);
            var ident = $"GroundClippers_{map.MapUid}.Item.Gbx";
            var bytes = FinishPlane.Build(Path.Combine(ApplicationConstants.ProjectDirectory, "data", "inventory", "FinishPlaneTemplate.Item.Gbx"), ident, [box]);

            var generatedPath = Path.Combine(Path.GetTempPath(), ident);
            File.WriteAllBytes(generatedPath, bytes);
            EmbedUtility.Embed(map, generatedPath);

            map.PlaceAnchoredObject(
                EmbedUtility.ExtractIdent(generatedPath),
                Vec3.Zero,
                Vec3.Zero,
                Vec3.Zero);
        }
    }
}
