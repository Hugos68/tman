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
            var templatePath = Path.Combine(ApplicationConstants.ProjectDirectory, "data", "items", "floor", "FinishFloorTemplate.Item.Gbx");
            var ident = $"FinishFloor_{map.MapUid}.Item.Gbx";

            var bytes = FloorFinishItem.Build(map, templatePath, ident);
            if (bytes is null)
            {
                return; // not a vista map, or no open-water cell: nothing to do
            }
            var generatedPath = Path.Combine(Path.GetTempPath(), ident);
            File.WriteAllBytes(generatedPath, bytes);
            EmbedUtility.Embed(map, generatedPath);
            map.PlaceAnchoredObject(
                EmbedUtility.ExtractIdent(generatedPath),
                Vec3.Zero,
                Vec3.Zero,
                Vec3.Zero);
            var placed = map.AnchoredObjects![^1];
            placed.WaypointSpecialProperty = new() { Tag = "Goal", Order = 0 };
        }
    }
}
