using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src.Alterations.Internal;
using tman.src.Application;
using tman.src.Core;
using tman.src.Inventory;
using tman.src.Positioning;

namespace tman.src.Alterations
{
    public class FloorFinish : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new InventoryRemoved(InventoryUtility.IsFinish, InventoryUtility.IsFinish),
            new CheckpointsCleaned(),
            new ConditionalAlteration(
                map => map.GetEnvironment() == "Stadium2020", 
                new ItemFinishes(new Dictionary<string, string>
                    {
                        ["DecoPlatformCurve3In"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishDecoPlatformCurve3In.Item.Gbx"),
                        ["DecoPlatformSlopeBaseCurve1InFullV0"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishDecoPlatformSlopeBaseCurve1InFullV0.Item.Gbx"),
                        ["DecoPlatformSlopeBaseCurve1OutV128"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishDecoPlatformSlopeBaseCurve1OutV128.Item.Gbx"),
                        ["DecoWallCurve1Pillar"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishDecoWallCurve1Pillar.Item.Gbx"),
                        ["DecoWallCurve2InPillar"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishDecoWallCurve2InPillar.Item.Gbx"),
                        ["DecoWallCurve2Pillar"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishDecoWallCurve2Pillar.Item.Gbx"),
                        ["DecoWallCurve3InPillar"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishDecoWallCurve3InPillar.Item.Gbx"),
                        ["DecoWallCurve3OutPillar"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishDecoWallCurve3OutPillar.Item.Gbx"),
                        ["DecoWallDiag1"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishDecoWallDiag1.Item.Gbx"),
                        ["DecoWallDiag1Pillar"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishDecoWallDiag1Pillar.Item.Gbx"),
                        ["SnowRoadPillarStraight"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishSnowRoadPillarStraight.Item.Gbx"),
                        ["StructureBase"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishStructureBase.Item.Gbx"),
                        ["StructurePillar"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishStructurePillar.Item.Gbx"),
                        ["TrackWallBranchYShaped2X3PillarV64"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishTrackWallBranchYShaped2X3PillarV64.Item.Gbx"),
                        ["TrackWallChicaneX2LeftPillarV64"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishTrackWallChicaneX2LeftPillarV64.Item.Gbx"),
                        ["TrackWallChicaneX3LeftPillarV64"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishTrackWallChicaneX3LeftPillarV64.Item.Gbx"),
                        ["TrackWallChicaneX3RightPillarV64"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishTrackWallChicaneX3RightPillarV64.Item.Gbx"),
                        ["TrackWallCurve1PillarV64"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishTrackWallCurve1PillarV64.Item.Gbx"),
                        ["TrackWallCurve2PillarV64"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishTrackWallCurve2PillarV64.Item.Gbx"),
                        ["TrackWallCurve3PillarV64"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishTrackWallCurve3PillarV64.Item.Gbx"),
                        ["TrackWallDeadendRoundPillarV64"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishTrackWallDeadendRoundPillarV64.Item.Gbx"),
                        ["TrackWallStraightPillar"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishTrackWallStraightPillar.Item.Gbx"),
                    },
                    blockPredicate: block => !block.IsFree & block.IsGround)),
            new MapNameEdited(name => name + " Floor Finish"),
        ];

        private static readonly Dictionary<string, (string[] Tiles, float Bottom, float Top)> OpenWater = new()
        {
            ["RedIsland"] = (["Water"], -13.6f, -3.7f),
            ["WhiteShore"] = (["Water"], -13.6f, -3.7f),
            ["BlueBay"] = (["Sea"], -10.1f, 5.1f),
            ["GreenCoast"] = (["Lake"], -13.6f, -3.7f),
        };
        
        private const float Thickness = 0.3f;

        private const int Ring = 16;

        protected override void Run(CGameCtnChallenge map)
        {
            var assets = Path.Combine(ApplicationConstants.ProjectDirectory, "assets");
            var environment = map.GetEnvironment();
            var boxes = environment == "Stadium2020"
                ? GrassBoxes(map, Path.Combine(assets, "data", "Blocks.json"))
                : WaterBoxes(map, environment, Path.Combine(assets, "data", "WaterFootprints.json"));
            if (boxes.Count == 0)
            {
                return;
            }
            FinishPlane.Place(map, Path.Combine(assets, "items", "floor-finish", "FinishPlaneTemplate.Item.Gbx"), $"FinishFloor_{map.MapUid}.Item.Gbx", boxes, environment == "Stadium2020");
            if (environment == "Stadium2020" && map.Decoration.Id.StartsWith("48x48"))
            {
                var path = Path.Combine(assets, "items", "floor-finish", "FinishStadiumPenaltyGrass.Item.Gbx");
                EmbedUtility.Embed(map, path);
                map.PlaceAnchoredObject(EmbedUtility.ExtractIdent(path), new Vec3(0, GrassLevel(map), 0), Vec3.Zero, Vec3.Zero);
            }
        }

        private static List<FinishPlane.Box> WaterBoxes(CGameCtnChallenge map, string environment, string footprintsPath)
        {
            var boxes = new List<FinishPlane.Box>();
            if (!OpenWater.TryGetValue(environment, out var water))
            {
                return boxes;
            }
            var footprints = LoadWaterFootprints(footprintsPath, environment);
            var cells = new Dictionary<int, List<int>>();
            var squares = new Dictionary<int, List<int>>();
            var seen = new HashSet<Int3>();
            foreach (var block in (map.Blocks ?? []).Concat(map.BakedBlocks ?? []).Where(b => !b.IsFree))
            {
                if (water.Tiles.Contains(block.BlockModel.Id))
                {
                    if (seen.Add(block.Coord))
                    {
                        Add(cells, block.Coord.Z, block.Coord.X);
                    }
                }
                else if (footprints.TryGetValue((block.BlockModel.Id, (uint)block.Flags & 0x0FE01FFF), out var deep))
                {
                    for (var zi = 0; zi < 8; zi++)
                    {
                        for (var xi = 0; xi < 8; xi++)
                        {
                            if ((deep & (1UL << (zi * 8 + xi))) == 0)
                            {
                                continue;
                            }
                            var (x, z) = block.Direction switch
                            {
                                Direction.East => (7 - zi, xi),
                                Direction.South => (7 - xi, 7 - zi),
                                Direction.West => (zi, 7 - xi),
                                _ => (xi, zi),
                            };
                            Add(squares, block.Coord.Z * 8 + z, block.Coord.X * 8 + x);
                        }
                    }
                }
            }
            foreach (var (z, xs) in cells)
            {
                foreach (var (x0, x1) in Runs(xs))
                {
                    boxes.Add(new FinishPlane.Box(x0 * MapConstants.CellSize, x1 * MapConstants.CellSize, z * MapConstants.CellSize, (z + 1) * MapConstants.CellSize, water.Bottom, water.Top));
                }
            }
            foreach (var (z, xs) in squares)
            {
                foreach (var (x0, x1) in Runs(xs))
                {
                    boxes.Add(new FinishPlane.Box(x0 * MapConstants.SubcellSize, x1 * MapConstants.SubcellSize, z * MapConstants.SubcellSize, (z + 1) * MapConstants.SubcellSize, water.Bottom, water.Top));
                }
            }
            var width = map.Size.X * MapConstants.CellSize;
            var depth = map.Size.Z * MapConstants.CellSize;
            var margin = Ring * MapConstants.CellSize;
            boxes.Add(new FinishPlane.Box(-margin, width + margin, -margin, 0, water.Bottom, water.Top));
            boxes.Add(new FinishPlane.Box(-margin, width + margin, depth, depth + margin, water.Bottom, water.Top));
            boxes.Add(new FinishPlane.Box(-margin, 0, 0, depth, water.Bottom, water.Top));
            boxes.Add(new FinishPlane.Box(width, width + margin, 0, depth, water.Bottom, water.Top));
            return boxes;
        }

        private static List<FinishPlane.Box> GrassBoxes(CGameCtnChallenge map, string blocksPath)
        {
            var boxes = new List<FinishPlane.Box>();
            var grass = (map.Blocks ?? []).Concat(map.BakedBlocks ?? []).Where(b => !b.IsFree && b.BlockModel.Id == "Grass").Select(b => b.Coord).Distinct().ToList();
            if (grass.Count == 0)
            {
                return boxes;
            }
            var blocks = Blocks.Load(blocksPath);
            var hidden = new HashSet<(int X, int Z)>();
            foreach (var block in (map.Blocks ?? []).Where(b => !b.IsFree && b.IsGround && b.BlockModel.Id != "Grass"))
            {
                var (sizeX, sizeZ, cells) = blocks.TryGetValue(block.BlockModel.Id, out var info) ? (info.SizeX, info.SizeZ, info.Footprint) : (1, 1, [(0, 0)]);
                foreach (var (ux, uz) in cells)
                {
                    var (x, z) = block.Direction switch
                    {
                        Direction.East => (sizeZ - 1 - uz, ux),
                        Direction.South => (sizeX - 1 - ux, sizeZ - 1 - uz),
                        Direction.West => (uz, sizeX - 1 - ux),
                        _ => (ux, uz),
                    };
                    hidden.Add((block.Coord.X + x, block.Coord.Z + z));
                }
            }
            var y = GrassLevel(map);
            var rows = new Dictionary<int, List<int>>();
            foreach (var (x, z) in grass.Select(c => (c.X, c.Z)).Distinct().Where(c => !hidden.Contains(c)))
            {
                Add(rows, z, x);
            }
            foreach (var (z, xs) in rows)
            {
                foreach (var (x0, x1) in Runs(xs))
                {
                    boxes.Add(new FinishPlane.Box(x0 * MapConstants.CellSize, x1 * MapConstants.CellSize, z * MapConstants.CellSize, (z + 1) * MapConstants.CellSize, y, y + Thickness));
                }
            }
            return boxes;
        }

        private static float GrassLevel(CGameCtnChallenge map) =>
            ((map.Blocks ?? []).Concat(map.BakedBlocks ?? []).Where(b => !b.IsFree && b.BlockModel.Id == "Grass").Min(b => b.Coord.Y) - MapUtility.GroundRow(map)) * MapConstants.CellHeight;

        private static IEnumerable<(int X0, int X1)> Runs(List<int> xs)
        {
            var sorted = xs.Distinct().OrderBy(x => x).ToList();
            for (var i = 0; i < sorted.Count;)
            {
                var start = sorted[i];
                var end = start;
                while (i + 1 < sorted.Count && sorted[i + 1] == end + 1)
                {
                    i++;
                    end = sorted[i];
                }
                i++;
                yield return (start, end + 1);
            }
        }

        private static Dictionary<(string Tile, uint Flags), ulong> LoadWaterFootprints(string path, string environment)
        {
            var table = new Dictionary<(string, uint), ulong>();
            var footprints = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(File.ReadAllText(path)) ?? new();
            foreach (var (tile, masks) in footprints.TryGetValue(environment, out var tiles) ? tiles : new())
            {
                foreach (var (flags, mask) in masks)
                {
                    table[(tile, uint.Parse(flags, NumberStyles.HexNumber, CultureInfo.InvariantCulture) & 0x0FE01FFF)] = ulong.Parse(mask, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                }
            }
            return table;
        }

        private static void Add<TKey>(Dictionary<TKey, List<int>> rows, TKey key, int x) where TKey : notnull
        {
            if (!rows.TryGetValue(key, out var xs))
            {
                rows[key] = xs = [];
            }
            xs.Add(x);
        }
    }
}
