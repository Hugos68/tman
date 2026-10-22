using System.Collections.Generic;
using System.IO;
using System.Linq;
using GBX.NET.Engines.Game;
using tman.src.Alterations.Internal;
using tman.src.Application;
using tman.src.Inventory;
using tman.src.Positioning;

namespace tman.src.Alterations
{
    public class Flags : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new WithoutValidation(),
            new WithoutInventory(InventoryUtility.IsFinish, InventoryUtility.IsFinish),
            new CheckpointsCleaned(),
        ];

        protected override void Run(CGameCtnChallenge map)
        {
            var flagsDirectory = Path.Combine(
                ApplicationConstants.ProjectDirectory,
                "data",
                "items",
                "flags");

            var flagItemPaths = Directory.EnumerateFiles(flagsDirectory, "*.Item.Gbx");

            foreach (string path in flagItemPaths)
            {
                EmbedUtility.Embed(map, path);
            }

            var flags = map.AnchoredObjects?
                .Where(item => item.ItemModel.Id.Contains("Flag"))
                .ToList() ?? [];

            foreach (var flag in flags)
            {
                var flagItemPath = flagItemPaths.FirstOrDefault(path => Path.GetFileName(path).Contains(flag.ItemModel.Id));
                if (flagItemPath is null)
                {
                    continue;
                }
                MapUtility.PlaceRelative(map, flag, new CGameCtnAnchoredObject()
                {
                    ItemModel = EmbedUtility.ExtractIdent(flagItemPath)
                });
            }
        }
    }
}