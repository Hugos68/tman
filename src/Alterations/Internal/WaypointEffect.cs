using System.Linq;
using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src.Inventory;
using tman.src.Positioning;

namespace tman.src.Alterations.Internal
{
    public abstract class WaypointEffect(string effect, Vec3 offset) : Alteration
    {
        protected abstract bool Filter(CGameCtnBlock block);

        protected abstract bool Filter(CGameCtnAnchoredObject item);

        protected override void Run(CGameCtnChallenge map)
        {
            var blocks = map.Blocks?.Where(Filter).ToList() ?? [];

            foreach (var block in blocks)
            {
                var offsetY = block.BlockModel.Id == "GateCheckpoint" ? 0 : -MapConstants.CellHeight * 1.75f;
                MapUtility.PlaceRelative(
                    map,
                    block,
                    new CGameCtnBlock { Name = $"GateSpecial{effect}" },
                    offset + new Vec3(0, offsetY, 0));

            }

            var items = map.AnchoredObjects?.Where(Filter).ToList() ?? [];

            foreach (var item in items)
            {
                var defaultOffset = InventoryUtility.IsStart(item) ? new Vec3(0, 0, MapConstants.ItemStartZOffsetZ) : new Vec3(0, 0, 0);
                MapUtility.PlaceRelative(
                    map,
                    item,
                    new CGameCtnAnchoredObject
                    {
                        ItemModel = new()
                        {
                            Id = InventoryUtility.ItemWaypointIdToItemEffectId(item.ItemModel.Id, effect),
                            Author = item.ItemModel.Author,
                            Collection = item.ItemModel.Collection
                        }
                    },
                    defaultOffset + offset);
            }
        }
    }
}