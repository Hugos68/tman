using System;
using System.IO;
using System.Linq;
using GBX.NET.Engines.Game;
using tman.src.Application;
using tman.src.Inventory;
using tman.src.Positioning;

namespace tman.src.Alterations
{
    public class Manslaughter : IAlteration
    {
        public void Apply(CGameCtnChallenge map)
        {
            var finishes = map.AnchoredObjects.RemoveAll(InventoryUtility.IsFinish);

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

            var stands = map.Blocks
                .Where(item => item.BlockModel.Id.Contains("Stand"))
                .ToList();

            foreach (var stand in stands)
            {
                var standItemPath = standItemPaths.FirstOrDefault(path => Path.GetFileName(path).Contains(stand.BlockModel.Id));
                Placement.PlaceRelative(map, stand, new CGameCtnAnchoredObject()
                {
                    ItemModel = EmbedUtility.ExtractIdent(standItemPath)
                });
            }
        }
    }
}