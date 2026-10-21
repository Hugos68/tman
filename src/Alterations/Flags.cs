using System.IO;
using System.Linq;
using GBX.NET.Engines.Game;
using tman.src.Application;
using tman.src.Inventory;
using tman.src.Positioning;

namespace tman.src.Alterations
{
    public class Flags : IAlteration
    {
        public void Apply(CGameCtnChallenge map)
        {
            new CheckpointsCleaned().Apply(map);

            var finishes = map.AnchoredObjects.RemoveAll(InventoryUtility.IsFinish);

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

            var flags = map.AnchoredObjects
                .Where(item => item.ItemModel.Id.Contains("Flag"))
                .ToList();

            foreach (var flag in flags)
            {
                var flagItemPath = flagItemPaths.FirstOrDefault(path => Path.GetFileName(path).Contains(flag.ItemModel.Id));
                Placement.PlaceRelative(map, flag, new()
                {
                    ItemModel = EmbedUtility.ExtractIdent(flagItemPath)
                });
            }
        }
    }
}