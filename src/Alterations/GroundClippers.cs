using System.Collections.Generic;
using System.Linq;
using tman.src.Alterations.Internal;
using tman.src.Core;
using tman.src.Maps;

namespace tman.src.Alterations
{
    public class GroundClippers : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new InventoryRemoved(placement => placement.IsFinish),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Ground Clippers")
        ];

        private static readonly Dictionary<Collections, (Block[] Tiles, float Bed)> Floors = new()
        {
            [Collections.RedIsland] = ([Blocks.Water, Blocks.WaterHill, Blocks.WaterShore1, Blocks.Dirt], 2.0f),
            [Collections.WhiteShore] = ([Blocks.Water, Blocks.WaterShore1, Blocks.Land], 2.0f),
            [Collections.BlueBay] = ([Blocks.Sea, Blocks.Beach, Blocks.SeaCliff13, Blocks.Sea_Land0_Land12], 4.0f),
            [Collections.GreenCoast] = ([Blocks.Lake, Blocks.LakeShore, Blocks.Grass], 2.0f),
            [Collections.Stadium] = ([Blocks.Grass], 0.0f),
        };

        private const float TopUnderFloor = 0.15f;
        private const float Thickness = 1.0f;
        private const float Margin = 6 * MapConstants.CellSize;

        protected override void Run(Map map)
        {
            if (!Floors.TryGetValue(map.Collection, out var floor))
            {
                return;
            }
            var rows = map.Terrain.Of(floor.Tiles)
                .Select(placement => placement.Cell.Y)
                .ToList();
            if (rows.Count == 0)
            {
                return;
            }
            var top = (rows.Min() - map.GroundRow) * MapConstants.CellHeight + floor.Bed - TopUnderFloor;
            var box = new FinishPlane.Box(-Margin, map.Size.X * MapConstants.CellSize + Margin, -Margin, map.Size.Z * MapConstants.CellSize + Margin, top - Thickness, top);
            FinishPlane.Place(
                map,
                Custom.FinishPlaneTemplate.FullPath(),
                $"GroundClippers_{map.Uid}.Item.Gbx",
                [box], false);
        }
    }
}
