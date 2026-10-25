
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GBX.NET.Engines.Game;
using tman.src.Alterations.Internal;
using tman.src.Application;
using tman.src.Core;
using tman.src.Inventory;
using tman.src.Positioning;

namespace tman.src.Alterations
{
    public class Manslaughter : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new InventoryRemoved(InventoryUtility.IsFinish, InventoryUtility.IsFinish),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Manslaughter")
        ];

        private static readonly Dictionary<string, string> BlockFinishes = new()
        {
            ["StandCornerIn"] = "FinishStandCornerIn.Item.Gbx",
            ["StandCornerOut"] = "FinishStandCornerOut.Item.Gbx",
            ["StandDiagIn"] = "FinishStandDiagIn.Item.Gbx",
            ["StandDiagOut"] = "FinishStandDiagOut.Item.Gbx",
            ["StandStraight"] = "FinishStandStraight.Item.Gbx",
        };

        private static readonly string StandsDirectory = Path.Combine(
            ApplicationConstants.ProjectDirectory,
            "assets",
            "items",
            "manslaughter");

        protected override void Run(CGameCtnChallenge map)
        {
            foreach (var (standId, finishFile) in BlockFinishes)
            {
                var sources = map.Blocks?
                    .Where(block => block.BlockModel.Id == standId)
                    .ToList() ?? [];

                if (sources.Count == 0)
                {
                    continue;
                }

                var finishPath = Path.Combine(StandsDirectory, finishFile);
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
