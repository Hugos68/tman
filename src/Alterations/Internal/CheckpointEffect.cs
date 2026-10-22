using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src.Alterations.Internal;
using tman.src.Inventory;

namespace tman.src.Alterations.Internal;

public class CheckpointEffect(string effect, Vec3 offset) : WaypointEffect(effect, offset)
{
    protected override bool Filter(CGameCtnBlock block) =>
        InventoryUtility.IsCheckpoint(block);

    protected override bool Filter(CGameCtnAnchoredObject item) =>
        InventoryUtility.IsCheckpoint(item);
}