using System.Collections.Generic;
using System.IO;
using tman.src.Alterations.Internal;
using tman.src.Application;
using tman.src.Core;
using tman.src.Inventory;

namespace tman.src.Alterations
{
    public class Moth : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new InventoryRemoved(InventoryUtility.IsFinish, InventoryUtility.IsFinish),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Moth"),
            new ItemFinishes(new Dictionary<string, string>
            {
                ["Lamp"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishLamp.Item.Gbx"),
                ["LampB"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishLampB.Item.Gbx"),
                ["LampC"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishLampC.Item.Gbx"),
                ["LampSmall"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishLampSmall.Item.Gbx"),
                ["LampSmallB"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishLampSmallB.Item.Gbx"),
                ["LampSmallC"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishLampSmallC.Item.Gbx"),
                ["ShowLight4Spots"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishShowLight4Spots.Item.Gbx"),
                ["ShowLightRamp4m"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishShowLightRamp4m.Item.Gbx"),
                ["ShowLightRamp8m"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishShowLightRamp8m.Item.Gbx"),
                ["ShowLightsV23"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishShowLightsV23.Item.Gbx"),
                ["ShowLightsV24"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishShowLightsV24.Item.Gbx"),
                ["ShowLightsV25"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishShowLightsV25.Item.Gbx"),
                ["ShowLightsV32"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishShowLightsV32.Item.Gbx"),
                ["ShowLightsV33"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishShowLightsV33.Item.Gbx"),
                ["ShowV23"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishShowV23.Item.Gbx"),
                ["ShowV24"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishShowV24.Item.Gbx"),
                ["ShowV25"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishShowV25.Item.Gbx"),
                ["ShowV35"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishShowV35.Item.Gbx"),
                ["ShowV36"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishShowV36.Item.Gbx"),
                ["SnowLampSupportLampion"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishSnowLampSupportLampion.Item.Gbx"),
                ["SnowLampSupportLamp1"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishSnowLampSupportLamp1.Item.Gbx"),
                ["SnowLampSupportLamp2"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishSnowLampSupportLamp2.Item.Gbx"),
                ["SnowTemple16m"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishSnowTemple16m.Item.Gbx"),
                ["SnowTemple24m"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishSnowTemple24m.Item.Gbx"),
                ["SnowTemple32m"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishSnowTemple32m.Item.Gbx"),
                ["SnowTempleCross"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishSnowTempleCross.Item.Gbx"),
                ["SnowTempleDeadEnd"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishSnowTempleDeadEnd.Item.Gbx"),
                ["SnowTempleRoadCorner"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishSnowTempleRoadCorner.Item.Gbx"),
                ["SnowTempleRoadStraight"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishSnowTempleRoadStraight.Item.Gbx"),
                ["SnowTempleRoadTShaped"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishSnowTempleRoadTShaped.Item.Gbx"),
                ["StageTechnicsLight"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishStageTechnicsLight.Item.Gbx"),
                ["StageTechnicsLightDeadend"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishStageTechnicsLightDeadend.Item.Gbx"),
                ["StageTechnicsLightDown"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishStageTechnicsLightDown.Item.Gbx"),
                ["StageTechnicsLightStraight"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishStageTechnicsLightStraight.Item.Gbx"),
                ["StageTechnicsLightToStructure"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "moth", "FinishStageTechnicsLightToStructure.Item.Gbx"),
            })
        ];
    }
}
