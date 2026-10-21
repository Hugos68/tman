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
            var blocks = map.Blocks.Where(block => InventoryUtility.IsStart(block) || InventoryUtility.IsCheckpoint(block)).ToList();
            foreach (var block in blocks)
            {
                PlacementUtility.PlaceRelative(map, block, new CGameCtnBlock() { Name = "GateSpecialNoEngine" }, new(0, -GridConstants.VerticalBlockSize, 0));
                PlacementUtility.PlaceRelative(map, block, new CGameCtnBlock() { Name = "GateSpecialTurbo" }, new(0, -GridConstants.VerticalBlockSize, 1));
            }
            var items = map.AnchoredObjects.Where(item => InventoryUtility.IsStart(item) || InventoryUtility.IsCheckpoint(item)).ToList();
            foreach (var item in items)
            {
                PlacementUtility.PlaceRelative(
                    map,
                    item,
                    new CGameCtnAnchoredObject()
                    {
                        ItemModel = new()
                        {
                            Id = InventoryUtility.ItemCheckpointIdToItemEffectId(item.ItemModel.Id, "NoEngine"),
                            Author = item.ItemModel.Author,
                            Collection = item.ItemModel.Collection
                        }
                    });
                PlacementUtility.PlaceRelative(
                    map,
                    item,
                    new CGameCtnAnchoredObject()
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
        }
    }
}