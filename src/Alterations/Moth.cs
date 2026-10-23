using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GBX.NET.Engines.Game;
using tman.src.Alterations.Internal;
using tman.src.Application;
using tman.src.Inventory;
using tman.src.Positioning;

namespace tman.src.Alterations
{
    public class Moth : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new WithoutValidation(),
            new WithoutInventory(InventoryUtility.IsFinish, InventoryUtility.IsFinish),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Moth")
        ];

        private static readonly Dictionary<string, string> ItemFinishes = new()
        {
            ["Lamp"] = "FinishLamp.Item.Gbx",
            ["LampB"] = "FinishLampB.Item.Gbx",
            ["LampC"] = "FinishLampC.Item.Gbx",
            ["LampSmall"] = "FinishLampSmall.Item.Gbx",
            ["LampSmallB"] = "FinishLampSmallB.Item.Gbx",
            ["LampSmallC"] = "FinishLampSmallC.Item.Gbx",
            ["ShowLight4Spots"] = "FinishShowLight4Spots.Item.Gbx",
            ["ShowLightRamp4m"] = "FinishShowLightRamp4m.Item.Gbx",
            ["ShowLightRamp8m"] = "FinishShowLightRamp8m.Item.Gbx",
            ["ShowLightsV23"] = "FinishShowLightsV23.Item.Gbx",
            ["ShowLightsV24"] = "FinishShowLightsV24.Item.Gbx",
            ["ShowLightsV25"] = "FinishShowLightsV25.Item.Gbx",
            ["ShowLightsV32"] = "FinishShowLightsV32.Item.Gbx",
            ["ShowLightsV33"] = "FinishShowLightsV33.Item.Gbx",
            ["ShowV23"] = "FinishShowV23.Item.Gbx",
            ["ShowV24"] = "FinishShowV24.Item.Gbx",
            ["ShowV25"] = "FinishShowV25.Item.Gbx",
            ["ShowV35"] = "FinishShowV35.Item.Gbx",
            ["ShowV36"] = "FinishShowV36.Item.Gbx",
            ["SnowLampSupportLampion"] = "FinishSnowLampSupportLampion.Item.Gbx",
            ["SnowLampSupportLamp1"] = "FinishSnowLampSupportLamp1.Item.Gbx",
            ["SnowLampSupportLamp2"] = "FinishSnowLampSupportLamp2.Item.Gbx",
            ["SnowTemple16m"] = "FinishSnowTemple16m.Item.Gbx",
            ["SnowTemple24m"] = "FinishSnowTemple24m.Item.Gbx",
            ["SnowTemple32m"] = "FinishSnowTemple32m.Item.Gbx",
            ["SnowTempleCross"] = "FinishSnowTempleCross.Item.Gbx",
            ["SnowTempleDeadEnd"] = "FinishSnowTempleDeadEnd.Item.Gbx",
            ["SnowTempleRoadCorner"] = "FinishSnowTempleRoadCorner.Item.Gbx",
            ["SnowTempleRoadStraight"] = "FinishSnowTempleRoadStraight.Item.Gbx",
            ["SnowTempleRoadTShaped"] = "FinishSnowTempleRoadTShaped.Item.Gbx",
        };

        private static readonly Dictionary<string, string> BlockFinishes = new()
        {
            ["StageTechnicsLight"] = "FinishStageTechnicsLight.Item.Gbx",
            ["StageTechnicsLightDeadend"] = "FinishStageTechnicsLightDeadend.Item.Gbx",
            ["StageTechnicsLightDown"] = "FinishStageTechnicsLightDown.Item.Gbx",
            ["StageTechnicsLightStraight"] = "FinishStageTechnicsLightStraight.Item.Gbx",
            ["StageTechnicsLightToStructure"] = "FinishStageTechnicsLightToStructure.Item.Gbx",
        };

        private static readonly string MothDirectory = Path.Combine(ApplicationConstants.ProjectDirectory, "data", "items", "moth");

        private static readonly Regex VariantRegex = new(@"V(\d+)$", RegexOptions.Compiled);

        protected override void Run(CGameCtnChallenge map)
        {
            foreach (var (itemId, finishFile) in ItemFinishes)
            {
                var match = VariantRegex.Match(itemId);
                var model = match.Success
                    ? itemId[..match.Index]
                    : itemId;
                int? variant = match.Success
                    ? int.Parse(match.Groups[1].Value)
                    : null;

                var sources = map.AnchoredObjects?
                    .Where(item =>
                        item.ItemModel.Id == model &&
                        (variant is null || Variant(item) == variant))
                    .ToList() ?? [];

                if (sources.Count == 0)
                {
                    continue;
                }

                var finishPath = Path.Combine(MothDirectory, finishFile);
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

            foreach (var (blockId, finishFile) in BlockFinishes)
            {
                var sources = map.Blocks?
                    .Where(block => block.Name == blockId)
                    .ToList() ?? [];

                if (sources.Count == 0)
                {
                    continue;
                }

                var finishPath = Path.Combine(MothDirectory, finishFile);
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

        private static int Variant(CGameCtnAnchoredObject item) =>
            (item.Flags >> 8) & 0xFF;
    }
}