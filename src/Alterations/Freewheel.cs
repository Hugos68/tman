using System;
using System.Linq;
using GBX.NET.Engines.Game;
using tman.src.Inventory;
using tman.src.Positioning;

namespace tman.src.Alterations
{

    public partial class Freewheel : IAlteration
    {

        public void Apply(CGameCtnChallenge map)
        {
            FreewheelStarts(map);
            FreewheelCheckpoints(map);
            map.MapName += " Freewheel";
        }

        private static void FreewheelStarts(CGameCtnChallenge map)
        {
            map.Blocks
                .Where(InventoryUtility.IsStart)
                .ToList()
                .ForEach(block =>
                {
                    Placement.PlaceRelative(map, block, new() { Name = "GateSpecialNoEngine" }, new(0, -GridConstants.BlockSize / 4, 0));
                    Placement.PlaceRelative(map, block, new() { Name = "GateSpecialTurbo" }, new(0, -GridConstants.BlockSize / 4, 1));
                });
            map.AnchoredObjects
                .Where(InventoryUtility.IsStart)
                .ToList()
                .ForEach(item =>
                {
                    Placement.PlaceRelative(
                        map,
                        item,
                        new()
                        {
                            ItemModel = new()
                            {
                                Id = GetReplacementItemId(item.ItemModel.Id, "NoEngine"),
                                Author = item.ItemModel.Author,
                                Collection = item.ItemModel.Collection
                            }
                        });
                    Placement.PlaceRelative(
                        map,
                        item,
                        new()
                        {
                            ItemModel = new()
                            {
                                Id = GetReplacementItemId(item.ItemModel.Id, "Turbo"),
                                Author = item.ItemModel.Author,
                                Collection = item.ItemModel.Collection
                            }
                        },
                        new(0, 0, 1));
                });
        }

        private static void FreewheelCheckpoints(CGameCtnChallenge map)
        {
            map.Blocks
                .Where(InventoryUtility.IsCheckpoint)
                .ToList()
                .ForEach(block =>
                {
                    Placement.PlaceRelative(map, block, new() { Name = "GateSpecialNoEngine" }, new(0, -GridConstants.BlockSize / 4, 0));
                    Placement.PlaceRelative(map, block, new() { Name = "GateSpecialTurbo" }, new(0, -GridConstants.BlockSize / 4, 1));
                });
            map.AnchoredObjects
                .Where(InventoryUtility.IsCheckpoint)
                .ToList()
                .ForEach(item =>
                {
                    Placement.PlaceRelative(
                        map,
                        item,
                        new()
                        {
                            ItemModel = new()
                            {
                                Id = GetReplacementItemId(item.ItemModel.Id, "NoEngine"),
                                Author = item.ItemModel.Author,
                                Collection = item.ItemModel.Collection
                            }
                        });
                    Placement.PlaceRelative(
                        map,
                        item,
                        new()
                        {
                            ItemModel = new()
                            {
                                Id = GetReplacementItemId(item.ItemModel.Id, "Turbo"),
                                Author = item.ItemModel.Author,
                                Collection = item.ItemModel.Collection
                            }
                        },
                        new(0, 0, 1));
                });
        }

        private static string GetReplacementItemId(string itemId, string effect)
        {
            return itemId
                .Replace("Center", "", StringComparison.Ordinal)
                .Replace("Left", "", StringComparison.Ordinal)
                .Replace("Right", "", StringComparison.Ordinal)
                .Replace("v2", "", StringComparison.Ordinal)
                .Replace("Checkpoint", "Special", StringComparison.Ordinal)
                .Replace("Start", "Special", StringComparison.Ordinal)
                + effect;
        }
    }
}