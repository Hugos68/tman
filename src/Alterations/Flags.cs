
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
            new ValidationRemoved(),
            new InventoryRemoved(InventoryUtility.IsFinish, InventoryUtility.IsFinish),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Flags")
        ];

        private static readonly Dictionary<string, string> ItemFinishes = new()
        {
            ["Flag8m"] = "FinishFlag8m.Item.Gbx",
            ["Flag16m"] = "FinishFlag16m.Item.Gbx",
        };

        private static readonly string FlagsDirectory = Path.Combine(
            ApplicationConstants.ProjectDirectory,
            "data",
            "items",
            "flags");

        protected override void Run(CGameCtnChallenge map)
        {
            foreach (var (flagId, finishFile) in ItemFinishes)
            {
                var sources = map.AnchoredObjects?
                    .Where(item => item.ItemModel.Id == flagId)
                    .ToList() ?? [];

                if (sources.Count == 0)
                {
                    continue;
                }

                var finishPath = Path.Combine(FlagsDirectory, finishFile);
                EmbedUtility.Embed(map, finishPath);

                var finish = new CGameCtnAnchoredObject
                {
                    ItemModel = EmbedUtility.ExtractIdent(finishPath)
                };

                foreach (var source in sources)
                {
                    MapUtility.PlaceRelative(map, source, finish);
                }
            }
        }
    }
}
