
using System;
using System.Numerics;
using GBX.NET;
using GBX.NET.Engines.Game;

namespace tman.src.Positioning;

public static class Placement
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
        var yaw = GetBlockYaw(reference.Direction);
        var rotatedOffset = RotateOffset(offset, yaw);

        block.Coord = reference.Coord;
        block.Direction = Direction.North;
        block.IsFree = true;
        block.YawPitchRoll = new Vec3(yaw, 0f, 0f);
        block.AbsolutePositionInMap = Add(position, rotatedOffset);

        map.PlaceBlock(block);
    }

    public static void PlaceRelative(
        CGameCtnChallenge map,
        CGameCtnAnchoredObject reference,
        CGameCtnAnchoredObject item,
        Vector3 offset = default)
    {
        var position = Add(reference.AbsolutePositionInMap, RotateOffset(offset, reference.YawPitchRoll.Y));

        map.PlaceAnchoredObject(
            item.ItemModel,
            position,
            reference.YawPitchRoll,
            Vec3.Zero);
    }

    public static void PlaceRelative(
        CGameCtnChallenge map,
        CGameCtnBlock reference,
        CGameCtnAnchoredObject item,
        Vector3 offset = default)
    {
        var position = Add(GetBlockPosition(reference, map), RotateOffset(offset, GetBlockYaw(reference.Direction)));
        var rotation = new Vec3(GetBlockYaw(reference.Direction), 0f, 0f);

        map.PlaceAnchoredObject(item.ItemModel, position, rotation, Vec3.Zero);
    }

    public static void PlaceRelative(
        CGameCtnChallenge map,
        CGameCtnAnchoredObject reference,
        CGameCtnBlock block,
        Vector3 offset = default)
    {
        var yaw = reference.YawPitchRoll.Y;
        var position = Add(reference.AbsolutePositionInMap, RotateOffset(offset, yaw));

        block.Coord = new(0, 0, 0);
        block.Direction = Direction.North;
        block.IsFree = true;
        block.YawPitchRoll = new Vec3(yaw, 0f, 0f);
        block.AbsolutePositionInMap = position;

        map.PlaceBlock(block);
    }

    private static Vec3 GetBlockPosition(CGameCtnBlock block, CGameCtnChallenge map)
    {
        var x = block.Coord.X * GridConstants.BlockSize;
        var y = (block.Coord.Y - GroundRow(map)) * GridConstants.VerticalBlockSize;
        var z = block.Coord.Z * GridConstants.BlockSize;

        if (block.Direction != Direction.West)
        {
            x += GridConstants.BlockSize;
        }

        if (block.Direction == Direction.South)
        {
            z += GridConstants.BlockSize;
        }

        return new Vec3(x, y, z);
    }

    private static float GetBlockYaw(Direction direction) =>
        direction switch
        {
            Direction.North => 0f,
            Direction.East => -MathF.PI / 2f,
            Direction.South => MathF.PI,
            Direction.West => MathF.PI / 2f,
            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };

    private static Vector3 RotateOffset(Vector3 offset, float yaw) =>
        new(
            offset.X * MathF.Cos(yaw) + offset.Z * MathF.Sin(yaw),
            offset.Y,
            -offset.X * MathF.Sin(yaw) + offset.Z * MathF.Cos(yaw));

    private static Vec3 Add(Vec3 position, Vector3 offset) =>
        new(
            position.X + offset.X,
            position.Y + offset.Y,
            position.Z + offset.Z);
}
