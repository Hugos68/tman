using System;
using System.Numerics;
using GBX.NET;
using GBX.NET.Engines.Game;

namespace tman.src.Positioning;

public static class PlacementUtility
{
    public static int GroundRow(CGameCtnChallenge map) =>
        map.Collection.ToString() switch
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
                block.Coord.X * GridConstants.BlockSize,
                (block.Coord.Y - GroundRow(map)) * GridConstants.VerticalBlockSize,
                block.Coord.Z * GridConstants.BlockSize));

        position.AddPosition(new Position(
            GetDirectionOffset(block.Direction),
            GetDirectionRotation(block.Direction)));

        return position;
    }

    private static Vec3 GetDirectionOffset(Direction direction) =>
        direction switch
        {
            Direction.North => Vec3.Zero,
            Direction.East => new(GridConstants.BlockSize, 0, 0),
            Direction.South => new(GridConstants.BlockSize, 0, GridConstants.BlockSize),
            Direction.West => new(0, 0, GridConstants.BlockSize),
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