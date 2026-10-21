using System;
using System.Linq;
using System.Text.RegularExpressions;
using GBX.NET.Engines.Game;
using tman.src.Positioning;

namespace tman.src.Alterations
{
    
    public partial class Freewheel : IAlteration
    {
        private static readonly Regex CheckpointAlignment = new("Center|Left|Right");

        public void Apply(CGameCtnChallenge map)
        {
            var checkpointBlocks = map.Blocks
                .Where(block => block.BlockModel.Id.Contains("Checkpoint"))
                .ToList();
            foreach (var checkpointBlock in checkpointBlocks)
            {
                Placement.PlaceRelative(map, checkpointBlock, new() { Name = "GateSpecialNoEngine" }, new(0, -GridConstants.BlockSize / 4, 0));
                Placement.PlaceRelative(map, checkpointBlock, new() { Name = "GateSpecialTurbo" }, new(0, -GridConstants.BlockSize / 4, 0));
            }
            var checkpointItems = map.AnchoredObjects
                .Where(item => item.ItemModel.Id.Contains("Checkpoint"))
                .ToList();
            foreach (var checkpointItem in checkpointItems)
            {

                Placement.PlaceRelative(
                    map,
                    checkpointItem,
                    new() { ItemModel = new() { Id = GetReplacementItemId(checkpointItem.ItemModel.Id, "NoEngine") } },
                    new(0, -GridConstants.BlockSize / 4, 0));
                Placement.PlaceRelative(
                    map,
                    checkpointItem,
                    new() { ItemModel = new() { Id = GetReplacementItemId(checkpointItem.ItemModel.Id, "Turbo") } },
                    new(0, -GridConstants.BlockSize / 4, 0));
            }
            map.MapName += " Freewheel";
        }
        private static string GetReplacementItemId(string itemId, string effect)
        {
            return itemId
                .Replace("Center", "", StringComparison.Ordinal)
                .Replace("Left", "", StringComparison.Ordinal)
                .Replace("Right", "", StringComparison.Ordinal)
                .Replace("v2", "", StringComparison.Ordinal)
                .Replace("Checkpoint", "Special", StringComparison.Ordinal)
                + effect;
        }
    }
}