using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GBX.NET.Engines.Game;
using tman.src.Alterations.Helpers;
using tman.src.Application;
using tman.src.Inventory;
using tman.src.Positioning;

namespace tman.src.Alterations
{
    public class Manslaughter : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new WithoutValidation(),
            new WithoutInventory(InventoryUtility.IsFinish, InventoryUtility.IsFinish),
            new StraightToTheFinish(),
        ];

        protected override void Run(CGameCtnChallenge map)
        {
            var standsDirectory = Path.Combine(
                ApplicationConstants.ProjectDirectory,
                "data",
                "items",
                "stands");

            var standItemPaths = Directory.EnumerateFiles(standsDirectory, "*.Item.Gbx");

            foreach (string path in standItemPaths)
            {
                EmbedUtility.Embed(map, path);
            }

            var stands = map.Blocks?
                .Where(item => item.BlockModel.Id.Contains("Stand"))
                .ToList() ?? [];

            foreach (var stand in stands)
            {
                var standItemPath = standItemPaths.FirstOrDefault(path => Path.GetFileName(path).Contains(stand.BlockModel.Id));
                if (standItemPath is null)
                {
                    continue;
                }
                PlacementUtility.PlaceRelative(map, stand, new CGameCtnAnchoredObject()
                {
                    ItemModel = EmbedUtility.ExtractIdent(standItemPath)
                });
            }
        }
    }
}