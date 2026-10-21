using System.Linq;
using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src.Inventory;
using tman.src.Positioning;

namespace tman.src.Alterations
{
    public partial class StartEffect(string effect, Vec3 offset) : IAlteration
    {
        public void Apply(CGameCtnChallenge map)
        {
            var blocks = map.Blocks.Where(InventoryUtility.IsStart).ToList();
            foreach (var block in blocks)
            {
                for (int i = 0; i < 4; i++)
                {
                    PlacementUtility.PlaceRelative(map, block, new CGameCtnBlock() { Name = $"GateExpandableSpecial{effect}" }, offset + new Vec3(0, GridConstants.VerticalBlockSize * i, 0));
                }
            }
            var items = map.AnchoredObjects.Where(InventoryUtility.IsStart).ToList();
            foreach (var item in items)
            {
                PlacementUtility.PlaceRelative(
                    map,
                    item,
                    new CGameCtnAnchoredObject()
                    {
                        ItemModel = new()
                        {
                            Id = InventoryUtility.ItemCheckpointIdToItemEffectId(item.ItemModel.Id, effect),
                            Author = item.ItemModel.Author,
                            Collection = item.ItemModel.Collection
                        }
                    }, offset);
            }
        }
    }
}