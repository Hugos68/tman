using System.Collections.Generic;
using System.Linq;
using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src.Inventory;
using tman.src.Positioning;

namespace tman.src.Alterations.Helpers
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
                for (int i = 0; i < 4; i++)
                {
                    MapUtility.PlaceRelative(
                        map,
                        block,
                        new CGameCtnBlock { Name = $"GateExpandableSpecial{effect}" },
                        offset + new Vec3(0, GridConstants.VerticalBlockSize * i, 0));
                }
            }

            var items = map.AnchoredObjects?.Where(Filter).ToList() ?? [];

            foreach (var item in items)
            {
                MapUtility.PlaceRelative(
                    map,
                    item,
                    new CGameCtnAnchoredObject
                    {
                        ItemModel = new()
                        {
                            Id = InventoryUtility.ItemCheckpointIdToItemEffectId(item.ItemModel.Id, effect),
                            Author = item.ItemModel.Author,
                            Collection = item.ItemModel.Collection
                        }
                    },
                    offset);
            }
        }
    }
}