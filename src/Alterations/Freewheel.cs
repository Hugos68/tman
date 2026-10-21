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
            var blocks = map.Blocks.Where(block => InventoryUtility.IsStart(block) || InventoryUtility.IsCheckpoint(block));
            foreach (var block in blocks)
            {
                Placement.PlaceRelative(map, block, new() { Name = "GateSpecialNoEngine" }, new(0, -GridConstants.VerticalBlockSize, 0));
                Placement.PlaceRelative(map, block, new() { Name = "GateSpecialTurbo" }, new(0, -GridConstants.VerticalBlockSize, 1));
            }
            var items = map.AnchoredObjects.Where(item => InventoryUtility.IsStart(item) || InventoryUtility.IsCheckpoint(item));
            foreach (var item in items)
            {
                Placement.PlaceRelative(
                    map,
                    item,
                    new()
                    {
                        ItemModel = new()
                        {
                            Id = InventoryUtility.ItemCheckpointIdToItemEffectId(item.ItemModel.Id, "NoEngine"),
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
                            Id = InventoryUtility.ItemCheckpointIdToItemEffectId(item.ItemModel.Id, "Turbo"),
                            Author = item.ItemModel.Author,
                            Collection = item.ItemModel.Collection
                        }
                    },
                    new(0, 0, 1));
            }
            map.MapName += " Freewheel";
        }
    }
}