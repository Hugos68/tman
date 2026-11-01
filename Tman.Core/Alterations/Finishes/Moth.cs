using System.Collections.Generic;
using Tman.Core.Alterations.Steps;
using Tman.Core.Inventory;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Finishes
{
    public class Moth : StandardAlteration
    {
        protected override string DisplayName => "Moth";

        protected override IEnumerable<Alteration> Steps =>
        [
            new InventoryRemoved(placement => placement.IsFinish),
            new CheckpointsCleaned(),
            new RelativelyPlacedItems(new Dictionary<Selector, Item>
            {
                [Items.Lamp] = Custom.FinishLamp,
                [Items.LampB] = Custom.FinishLampB,
                [Items.LampC] = Custom.FinishLampC,
                [Items.LampSmall] = Custom.FinishLampSmall,
                [Items.LampSmallB] = Custom.FinishLampSmallB,
                [Items.LampSmallC] = Custom.FinishLampSmallC,
                [Items.ShowLight4Spots] = Custom.FinishShowLight4Spots,
                [Items.ShowLightRamp4m] = Custom.FinishShowLightRamp4m,
                [Items.ShowLightRamp8m] = Custom.FinishShowLightRamp8m,
                [Items.ShowLights.Variant(23)] = Custom.FinishShowLightsV23,
                [Items.ShowLights.Variant(24)] = Custom.FinishShowLightsV24,
                [Items.ShowLights.Variant(25)] = Custom.FinishShowLightsV25,
                [Items.ShowLights.Variant(32)] = Custom.FinishShowLightsV32,
                [Items.ShowLights.Variant(33)] = Custom.FinishShowLightsV33,
                [Items.Show.Variant(23)] = Custom.FinishShowV23,
                [Items.Show.Variant(24)] = Custom.FinishShowV24,
                [Items.Show.Variant(25)] = Custom.FinishShowV25,
                [Items.Show.Variant(35)] = Custom.FinishShowV35,
                [Items.Show.Variant(36)] = Custom.FinishShowV36,
                [Items.SnowLampSupportLampion] = Custom.FinishSnowLampSupportLampion,
                [Items.SnowLampSupportLamp1] = Custom.FinishSnowLampSupportLamp1,
                [Items.SnowLampSupportLamp2] = Custom.FinishSnowLampSupportLamp2,
                [Items.SnowTemple16m] = Custom.FinishSnowTemple16m,
                [Items.SnowTemple24m] = Custom.FinishSnowTemple24m,
                [Items.SnowTemple32m] = Custom.FinishSnowTemple32m,
                [Items.SnowTempleCross] = Custom.FinishSnowTempleCross,
                [Items.SnowTempleDeadEnd] = Custom.FinishSnowTempleDeadEnd,
                [Items.SnowTempleRoadCorner] = Custom.FinishSnowTempleRoadCorner,
                [Items.SnowTempleRoadStraight] = Custom.FinishSnowTempleRoadStraight,
                [Items.SnowTempleRoadTShaped] = Custom.FinishSnowTempleRoadTShaped,
                [Blocks.StageTechnicsLight] = Custom.FinishStageTechnicsLight,
                [Blocks.StageTechnicsLightDeadend] = Custom.FinishStageTechnicsLightDeadend,
                [Blocks.StageTechnicsLightDown] = Custom.FinishStageTechnicsLightDown,
                [Blocks.StageTechnicsLightStraight] = Custom.FinishStageTechnicsLightStraight,
                [Blocks.StageTechnicsLightToStructure] = Custom.FinishStageTechnicsLightToStructure,
            })
        ];
    }
}
