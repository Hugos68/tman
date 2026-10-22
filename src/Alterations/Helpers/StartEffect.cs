using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src.Alterations.Helpers;
using tman.src.Inventory;

namespace tman.src.Alterations.Helpers;

public class StartEffect(string effect, Vec3 offset) : WaypointEffect(effect, offset)
{
    protected override bool Filter(CGameCtnBlock block) =>
        InventoryUtility.IsStart(block);

    protected override bool Filter(CGameCtnAnchoredObject item) =>
        InventoryUtility.IsStart(item);
}