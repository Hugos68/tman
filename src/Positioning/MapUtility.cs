using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src.Application;

namespace tman.src.Positioning
{
    public static class MapUtility
    {
        private static readonly Lazy<Dictionary<string, (int X, int Z)>> BlockSizes = new(LoadBlockSizes);

        public static int GroundRow(CGameCtnChallenge map) =>
            map.GetEnvironment() switch
            {
                "RedIsland" or "WhiteShore" => 15,
                "BlueBay" or "GreenCoast" => 5,
                _ => map.DecoBaseHeightOffset
            };

        public static void PlaceRelative(
            CGameCtnChallenge map,
            CGameCtnBlock reference,
            CGameCtnBlock block,
            Vector3 offset = default)
        {
            var position = GetBlockPosition(reference, map);
            position.Move(ToVec3(offset));
            block.Coord = reference.Coord;
            block.Direction = Direction.North;
            block.IsFree = true;
            block.AbsolutePositionInMap = position.Coords;
            block.YawPitchRoll = position.YawPitchRoll;
            map.PlaceBlock(block);
        }

        public static void PlaceRelative(
            CGameCtnChallenge map,
            CGameCtnAnchoredObject reference,
            CGameCtnAnchoredObject item,
            Vector3 offset = default)
        {
            var position = new Position(
                reference.AbsolutePositionInMap,
                reference.YawPitchRoll);
            position.Move(ToVec3(offset));
            map.PlaceAnchoredObject(
                item.ItemModel,
                position.Coords,
                position.YawPitchRoll,
                Vec3.Zero);
        }

        public static void PlaceRelative(
            CGameCtnChallenge map,
            CGameCtnBlock reference,
            CGameCtnAnchoredObject item,
            Vector3 offset = default)
        {
            var position = GetBlockPosition(reference, map);
            position.Move(ToVec3(offset));
            map.PlaceAnchoredObject(
                item.ItemModel,
                position.Coords,
                position.YawPitchRoll,
                Vec3.Zero);
        }

        public static void PlaceRelative(
            CGameCtnChallenge map,
            CGameCtnAnchoredObject reference,
            CGameCtnBlock block,
            Vector3 offset = default)
        {
            var position = new Position(
                reference.AbsolutePositionInMap,
                reference.YawPitchRoll);
            position.Move(ToVec3(offset));
            block.Coord = new(0, 0, 0);
            block.Direction = Direction.North;
            block.IsFree = true;
            block.AbsolutePositionInMap = position.Coords;
            block.YawPitchRoll = position.YawPitchRoll;
            map.PlaceBlock(block);
        }

        private static Position GetBlockPosition(
            CGameCtnBlock block,
            CGameCtnChallenge map)
        {
            if (block.IsFree)
            {
                return new Position(
                    block.AbsolutePositionInMap,
                    block.YawPitchRoll);
            }
            var position = new Position(
                new Vec3(
                    block.Coord.X * MapConstants.CellSize,
                    (block.Coord.Y - GroundRow(map)) * MapConstants.CellHeight,
                    block.Coord.Z * MapConstants.CellSize));
            position.AddPosition(new Position(
                GetDirectionOffset(block.Direction, GetBlockSize(block)),
                GetDirectionRotation(block.Direction)));
            return position;
        }

        private static (int X, int Z) GetBlockSize(CGameCtnBlock block) =>
            BlockSizes.Value.TryGetValue(block.BlockModel.Id, out var size) ? size : (1, 1);

        private static Dictionary<string, (int X, int Z)> LoadBlockSizes()
        {
            var sizes = new Dictionary<string, (int X, int Z)>(StringComparer.OrdinalIgnoreCase);
            var path = Path.Combine(ApplicationConstants.ProjectDirectory, "data", "footprints", "BlockSizes.tsv");
            foreach (var fields in File.ReadLines(path).Skip(1).Select(line => line.Split('\t')))
            {
                sizes[fields[0]] = (int.Parse(fields[1]), int.Parse(fields[2]));
            }
            return sizes;
        }

        private static Vec3 GetDirectionOffset(Direction direction, (int X, int Z) size) =>
            direction switch
            {
                Direction.North => Vec3.Zero,
                Direction.East => new(size.Z * MapConstants.CellSize, 0, 0),
                Direction.South => new(size.X * MapConstants.CellSize, 0, size.Z * MapConstants.CellSize),
                Direction.West => new(0, 0, size.X * MapConstants.CellSize),
                _ => throw new ArgumentOutOfRangeException(nameof(direction))
            };

        private static Vec3 GetDirectionRotation(Direction direction) =>
            direction switch
            {
                Direction.North => Vec3.Zero,
                Direction.East => new(-MathF.PI / 2, 0, 0),
                Direction.South => new(MathF.PI, 0, 0),
                Direction.West => new(MathF.PI / 2, 0, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(direction))
            };

        private static Vec3 ToVec3(Vector3 vector) =>
            new(vector.X, vector.Y, vector.Z);
    }
}
