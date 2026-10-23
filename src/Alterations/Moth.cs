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
    public class Moth : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new WithoutValidation(),
            new WithoutInventory(InventoryUtility.IsFinish, InventoryUtility.IsFinish),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Moth")
        ];

        private static readonly Dictionary<(string Model, int? Variant), string> ItemCarriers = new()
        {
            [("Lamp", null)] = "FinishLamp.Item.Gbx",
            [("LampB", null)] = "FinishLampB.Item.Gbx",
            [("LampC", null)] = "FinishLampC.Item.Gbx",
            [("LampSmall", null)] = "FinishLampSmall.Item.Gbx",
            [("LampSmallB", null)] = "FinishLampSmallB.Item.Gbx",
            [("LampSmallC", null)] = "FinishLampSmallC.Item.Gbx",
            [("ShowLight4Spots", null)] = "FinishShowLight4Spots.Item.Gbx",
            [("ShowLightRamp4m", null)] = "FinishShowLightRamp4m.Item.Gbx",
            [("ShowLightRamp8m", null)] = "FinishShowLightRamp8m.Item.Gbx",
            [("ShowLights", 23)] = "FinishShowLightsV23.Item.Gbx",   // Light4Spots head
            [("ShowLights", 24)] = "FinishShowLightsV24.Item.Gbx",   // LightRamp4m bar
            [("ShowLights", 25)] = "FinishShowLightsV25.Item.Gbx",   // LightRamp8m bar
            [("ShowLights", 32)] = "FinishShowLightsV32.Item.Gbx",   // LightsFront
            [("ShowLights", 33)] = "FinishShowLightsV33.Item.Gbx",
            [("Show", 23)] = "FinishShowV23.Item.Gbx",
            [("Show", 24)] = "FinishShowV24.Item.Gbx",
            [("Show", 25)] = "FinishShowV25.Item.Gbx",
            [("Show", 35)] = "FinishShowV35.Item.Gbx",
            [("Show", 36)] = "FinishShowV36.Item.Gbx",
            [("SnowLampSupportLampion", null)] = "FinishSnowLampSupportLampion.Item.Gbx",
            [("SnowLampSupportLamp1", null)] = "FinishSnowLampSupportLamp1.Item.Gbx",
            [("SnowLampSupportLamp2", null)] = "FinishSnowLampSupportLamp2.Item.Gbx",
            [("SnowTemple16m", null)] = "FinishSnowTemple16m.Item.Gbx",
            [("SnowTemple24m", null)] = "FinishSnowTemple24m.Item.Gbx",
            [("SnowTemple32m", null)] = "FinishSnowTemple32m.Item.Gbx",
            [("SnowTempleCross", null)] = "FinishSnowTempleCross.Item.Gbx",
            [("SnowTempleDeadEnd", null)] = "FinishSnowTempleDeadEnd.Item.Gbx",
            [("SnowTempleRoadCorner", null)] = "FinishSnowTempleRoadCorner.Item.Gbx",
            [("SnowTempleRoadStraight", null)] = "FinishSnowTempleRoadStraight.Item.Gbx",
            [("SnowTempleRoadTShaped", null)] = "FinishSnowTempleRoadTShaped.Item.Gbx",
        };

        private static readonly Dictionary<string, string> BlockCarriers = new()
        {
            ["StageTechnicsLight"] = "FinishStageTechnicsLight.Item.Gbx",
            ["StageTechnicsLightDeadend"] = "FinishStageTechnicsLightDeadend.Item.Gbx",
            ["StageTechnicsLightDown"] = "FinishStageTechnicsLightDown.Item.Gbx",
            ["StageTechnicsLightStraight"] = "FinishStageTechnicsLightStraight.Item.Gbx",
            ["StageTechnicsLightToStructure"] = "FinishStageTechnicsLightToStructure.Item.Gbx",
        };

        protected override void Run(CGameCtnChallenge map)
        {
            var mothDirectory = Path.Combine(ApplicationConstants.ProjectDirectory, "data", "items", "lights");
            
            var carrierPaths = Directory.EnumerateFiles(mothDirectory, "*.Item.Gbx")
                .ToDictionary(path => Path.GetFileName(path), path => path);

            var used = new HashSet<string>();

            var sources = map.AnchoredObjects?.ToList() ?? [];
            foreach (var source in sources)
            {
                var carrierFile = CarrierFor(source.ItemModel.Id, Variant(source));
                if (carrierFile is null || !carrierPaths.TryGetValue(carrierFile, out var carrierPath))
                {
                    continue;
                }
                Embed(map, carrierPath, used);
                MapUtility.PlaceRelative(map, source, Carrier(carrierPath));
            }

            foreach (var block in map.Blocks?.ToList() ?? [])
            {
                if (!BlockCarriers.TryGetValue(block.Name, out var carrierFile) || !carrierPaths.TryGetValue(carrierFile, out var carrierPath))
                {
                    continue;
                }
                Embed(map, carrierPath, used);
                MapUtility.PlaceRelative(map, block, Carrier(carrierPath));
            }
        }

        private static string? CarrierFor(string model, int variant)
        {
            if (ItemCarriers.TryGetValue((model, variant), out var byVariant))
            {
                return byVariant;
            }
            return ItemCarriers.TryGetValue((model, null), out var any) ? any : null;
        }

        private static int Variant(CGameCtnAnchoredObject item) => (item.Flags >> 8) & 0xFF;

        private static CGameCtnAnchoredObject Carrier(string carrierPath) => new()
        {
            ItemModel = EmbedUtility.ExtractIdent(carrierPath),
            WaypointSpecialProperty = new() { Tag = "Goal", Order = 0 },
        };

        private static void Embed(CGameCtnChallenge map, string carrierPath, HashSet<string> used)
        {
            if (used.Add(carrierPath))
            {
                EmbedUtility.Embed(map, carrierPath);
            }
        }
    }
}
