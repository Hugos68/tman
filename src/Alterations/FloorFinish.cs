using System.Collections.Generic;
using System.IO;
using System.Linq;
using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src.Alterations.Internal;
using tman.src.Application;
using tman.src.Inventory;
using tman.src.Positioning;

namespace tman.src.Alterations
{
    public class FloorFinish : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new WithoutValidation(),
            new WithoutInventory(InventoryUtility.IsFinish, InventoryUtility.IsFinish),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Floor Finish")
        ];

        private static readonly Dictionary<(string Block, int Variant), string> Cutouts = new()
        {
            [("DecoPlatformCurve3In", 0)] = "FloorFinishDecoPlatformCurve3In.Item.Gbx",
            [("DecoPlatformSlopeBaseCurve1InFull", 0)] = "FloorFinishDecoPlatformSlopeBaseCurve1InFull.Item.Gbx",
            [("DecoPlatformSlopeBaseCurve1Out", 128)] = "FloorFinishDecoPlatformSlopeBaseCurve1OutV128.Item.Gbx",
            [("DecoWallCurve1Pillar", 0)] = "FloorFinishDecoWallCurve1Pillar.Item.Gbx",
            [("DecoWallCurve2InPillar", 0)] = "FloorFinishDecoWallCurve2InPillar.Item.Gbx",
            [("DecoWallCurve2Pillar", 0)] = "FloorFinishDecoWallCurve2Pillar.Item.Gbx",
            [("DecoWallCurve3InPillar", 0)] = "FloorFinishDecoWallCurve3InPillar.Item.Gbx",
            [("DecoWallCurve3OutPillar", 0)] = "FloorFinishDecoWallCurve3OutPillar.Item.Gbx",
            [("DecoWallDiag1", 0)] = "FloorFinishDecoWallDiag1.Item.Gbx",
            [("DecoWallDiag1Pillar", 0)] = "FloorFinishDecoWallDiag1Pillar.Item.Gbx",
            [("SnowRoadPillarStraight", 0)] = "FloorFinishSnowRoadPillarStraight.Item.Gbx",
            [("StructureBase", 0)] = "FloorFinishStructureBase.Item.Gbx",
            [("StructurePillar", 0)] = "FloorFinishStructurePillar.Item.Gbx",
            [("TrackWallBranchYShaped2X3Pillar", 64)] = "FloorFinishTrackWallBranchYShaped2X3PillarV64.Item.Gbx",
            [("TrackWallChicaneX2LeftPillar", 64)] = "FloorFinishTrackWallChicaneX2LeftPillarV64.Item.Gbx",
            [("TrackWallChicaneX3LeftPillar", 64)] = "FloorFinishTrackWallChicaneX3LeftPillarV64.Item.Gbx",
            [("TrackWallChicaneX3RightPillar", 64)] = "FloorFinishTrackWallChicaneX3RightPillarV64.Item.Gbx",
            [("TrackWallCurve1Pillar", 64)] = "FloorFinishTrackWallCurve1PillarV64.Item.Gbx",
            [("TrackWallCurve2Pillar", 64)] = "FloorFinishTrackWallCurve2PillarV64.Item.Gbx",
            [("TrackWallCurve3Pillar", 64)] = "FloorFinishTrackWallCurve3PillarV64.Item.Gbx",
            [("TrackWallDeadendRoundPillar", 64)] = "FloorFinishTrackWallDeadendRoundPillarV64.Item.Gbx",
        };

        protected override void Run(CGameCtnChallenge map)
        {
            var floorDirectory = Path.Combine(ApplicationConstants.ProjectDirectory, "data", "inventory", "floor-finish");
            var footprintsDirectory = Path.Combine(ApplicationConstants.ProjectDirectory, "data", "footprints");
            var ident = $"FinishFloor_{map.MapUid}.Item.Gbx";

            var bytes = FinishPlane.Build(map, Path.Combine(ApplicationConstants.ProjectDirectory, "data", "inventory", "FinishPlaneTemplate.Item.Gbx"), footprintsDirectory, ident);
            if (bytes is null)
            {
                return;
            }

            var generatedPath = Path.Combine(Path.GetTempPath(), ident);
            File.WriteAllBytes(generatedPath, bytes);
            EmbedUtility.Embed(map, generatedPath);

            map.PlaceAnchoredObject(
                EmbedUtility.ExtractIdent(generatedPath),
                Vec3.Zero,
                Vec3.Zero,
                Vec3.Zero);

            if (map.GetEnvironment() != "Stadium2020")
            {
                return;
            }

            var embedded = new HashSet<string>();
            foreach (var block in (map.Blocks ?? []).Where(b => !b.IsFree && b.IsGround && b.BlockModel.Id != "Grass"))
            {
                if (!Cutouts.TryGetValue((block.BlockModel.Id, FinishPlane.Variant(block)), out var file))
                {
                    continue;
                }
                var path = Path.Combine(floorDirectory, file);
                if (embedded.Add(file))
                {
                    EmbedUtility.Embed(map, path);
                }
                MapUtility.PlaceRelative(map, block, new CGameCtnAnchoredObject { ItemModel = EmbedUtility.ExtractIdent(path) });
            }
        }
    }
}
