using System.Collections.Generic;
using Tman.Core.Alterations.Internal;
using Tman.Core.Inventory;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Finishes
{
    public class Screens : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new InventoryRemoved(placement => placement.IsFinish),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Screens"),
            new ItemFinishes(new Dictionary<Selector, Item>
            {
                [Blocks.TechnicsScreen1x1Straight] = Custom.FinishScreen1x1Straight,
                [Blocks.TechnicsScreen1x1StraightX2] = Custom.FinishScreen1x1StraightX2,
                [Blocks.TechnicsScreen1x1StraightX3] = Custom.FinishScreen1x1StraightX3,
                [Blocks.TechnicsScreen1x1Curve1In] = Custom.FinishScreen1x1Curve1In,
                [Blocks.TechnicsScreen1x1Curve2In] = Custom.FinishScreen1x1Curve2In,
                [Blocks.TechnicsScreen1x1Curve2Out] = Custom.FinishScreen1x1Curve2Out,
                [Blocks.TechnicsScreen1x1Curve3Out] = Custom.FinishScreen1x1Curve3Out,
                [Blocks.TechnicsScreen2x1Straight] = Custom.FinishScreen2x1Straight,
                [Blocks.TechnicsScreen2x1StraightSmall] = Custom.FinishScreen2x1StraightSmall,
                [Blocks.TechnicsScreen2x1StraightX2] = Custom.FinishScreen2x1StraightX2,
                [Blocks.TechnicsScreen2x1StraightX3] = Custom.FinishScreen2x1StraightX3,
                [Blocks.TechnicsScreen2x1Curve1In] = Custom.FinishScreen2x1Curve1In,
                [Blocks.TechnicsScreen2x1Curve2In] = Custom.FinishScreen2x1Curve2In,
                [Blocks.TechnicsScreen2x1Curve2Out] = Custom.FinishScreen2x1Curve2Out,
                [Blocks.TechnicsScreen2x1Curve3In] = Custom.FinishScreen2x1Curve3In,
                [Blocks.TechnicsScreen2x1Curve3Out] = Custom.FinishScreen2x1Curve3Out,
                [Blocks.TechnicsScreen2x1Curve4Out] = Custom.FinishScreen2x1Curve4Out,
                [Blocks.TechnicsScreen2x3Straight] = Custom.FinishScreen2x3Straight,
                [Blocks.TechnicsScreen2x3StraightSmall] = Custom.FinishScreen2x3StraightSmall,
                [Blocks.TechnicsScreen2x3StraightX2] = Custom.FinishScreen2x3StraightX2,
                [Blocks.TechnicsScreen4x1Straight] = Custom.FinishScreen4x1Straight,
                [Blocks.TechnicsScreen4x1StraightX2] = Custom.FinishScreen4x1StraightX2,
                [Blocks.TechnicsScreen4x1StraightX3] = Custom.FinishScreen4x1StraightX3,
                [Blocks.TechnicsScreen4x1Curve1In] = Custom.FinishScreen4x1Curve1In,
                [Blocks.TechnicsScreen4x1Curve2In] = Custom.FinishScreen4x1Curve2In,
                [Blocks.TechnicsScreen4x1Curve2Out] = Custom.FinishScreen4x1Curve2Out,
                [Blocks.TechnicsScreen4x1Curve3In] = Custom.FinishScreen4x1Curve3In,
                [Blocks.TechnicsScreen4x1Curve3Out] = Custom.FinishScreen4x1Curve3Out,
                [Blocks.TechnicsScreen4x1Curve4Out] = Custom.FinishScreen4x1Curve4Out,
                [Blocks.TechnicsScreen155Straight] = Custom.FinishScreen155Straight,
                [Blocks.TechnicsScreen155StraightX2] = Custom.FinishScreen155StraightX2,
                [Blocks.TechnicsScreen155StraightX4] = Custom.FinishScreen155StraightX4,
                [Blocks.TechnicsScreen16x9Straight] = Custom.FinishScreen16x9Straight,
                [Blocks.TechnicsScreen16x9StraightX2] = Custom.FinishScreen16x9StraightX2,
                [Blocks.TechnicsScreen6x1StraightX3] = Custom.FinishScreen6x1StraightX3,
                [Items.Screen1x1] = Custom.FinishScreen1x1,
                [Items.Screen1x1Small] = Custom.FinishScreen1x1Small,
                [Items.Screen2x1] = Custom.FinishScreen2x1,
                [Items.Screen2x1Small] = Custom.FinishScreen2x1Small,
                [Items.Screen2x3] = Custom.FinishScreen2x3,
                [Items.Screen2x3Small] = Custom.FinishScreen2x3Small,
                [Items.Screen2x3Big] = Custom.FinishScreen2x3Big,
                [Items.Screen2x3VeryBig] = Custom.FinishScreen2x3VeryBig,
                [Items.Screen4x1] = Custom.FinishScreen4x1,
                [Items.Screen4x1Small] = Custom.FinishScreen4x1Small,
                [Items.Screen155] = Custom.FinishScreen155,
                [Items.Screen155Small] = Custom.FinishScreen155Small,
                [Items.Screen16x9] = Custom.FinishScreen16x9,
                [Items.Screen16x9Small] = Custom.FinishScreen16x9Small,
                [Items.Screen16x9VeryBig] = Custom.FinishScreen16x9VeryBig,
                [Items.RaceScreen6x1] = Custom.FinishRaceScreen6x1,
                [Items.RaceScreen6x1Small] = Custom.FinishRaceScreen6x1Small,
                [Items.ShowScreen.Variant(5)] = Custom.FinishShowScreen5,
                [Items.ShowScreen.Variant(6)] = Custom.FinishShowScreen6,
                [Items.ShowScreen.Variant(7)] = Custom.FinishShowScreen7,
                [Items.ShowScreen.Variant(8)] = Custom.FinishShowScreen8,
                [Items.ShowScreen.Variant(9)] = Custom.FinishShowScreen9,
                [Items.ShowScreen.Variant(10)] = Custom.FinishShowScreen10,
                [Items.ShowRace.Variant(8)] = Custom.FinishShowRace8,
                [Items.ShowRace.Variant(9)] = Custom.FinishShowRace9,
                [Items.ShowRace.Variant(10)] = Custom.FinishShowRace10,
                [Items.ShowRace.Variant(11)] = Custom.FinishShowRace11,
                [Items.ShowRace.Variant(12)] = Custom.FinishShowRace12,
            })
        ];
    }
}