using System.IO;
using System.Linq;
using GBX.NET.Engines.Game;
using tman.src.Positioning;

namespace tman.src.Alterations
{
    public class Flags : IAlteration
    {
        public void Apply(CGameCtnChallenge map)
        {
            new CheckpointsCleaned().Apply(map);
            map.UpdateEmbeddedZipData(zip =>
            {
                foreach (string filePath in Directory.EnumerateFiles(Path.Combine("data", "flags"), "*.Item.Gbx"))
                {
                    var entry = zip.CreateEntry(Path.Combine("items", "flags", Path.GetFileName(filePath)));
                    using var entryStream = entry.Open();
                    using var fileStream = File.OpenRead(filePath);
                    fileStream.CopyTo(entryStream);
                }
            });
            var flags = map.AnchoredObjects.Where(item => item.ItemModel.Id.Contains("Flag"));
            foreach (var flag in flags)
            {
                Placement.PlaceRelative(map, flag, new()
                {
                    ItemModel = new()
                    {
                        Id = $"items/flags/Finish{flag.ItemModel.Id}"
                    }
                });
            }
            map.MapName = " Flags";
        }
    }
}