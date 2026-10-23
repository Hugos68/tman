using System.Collections.Generic;
using System.IO;
using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src.Alterations.Internal;
using tman.src.Application;
using tman.src.Inventory;
using tman.src.Positioning;

namespace tman.src.Alterations
{
    public class FloorFinish : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new WithoutValidation(),
            new WithoutInventory(InventoryUtility.IsFinish, InventoryUtility.IsFinish),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Floor Finish")
        ];

        protected override void Run(CGameCtnChallenge map)
        {
            var templatePath = Path.Combine(ApplicationConstants.ProjectDirectory, "data", "items", "floor-finish", "FinishFloorTemplate.Item.Gbx");
            var ident = $"FinishFloor_{map.MapUid}.Item.Gbx";
            var floorFinishItem = FloorFinishItem.Build(map, templatePath, ident);
            if (floorFinishItem is null)
            {
                return;
            }
            var itemPath = Path.Combine(Path.GetTempPath(), ident);
            File.WriteAllBytes(itemPath, floorFinishItem);
            EmbedUtility.Embed(map, itemPath);
            map.PlaceAnchoredObject(EmbedUtility.ExtractIdent(itemPath), Vec3.Zero, Vec3.Zero, Vec3.Zero);
        }
    }
}
