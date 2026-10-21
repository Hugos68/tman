
using System;
using System.Numerics;
using GBX.NET;
using GBX.NET.Engines.Game;
using Microsoft.VisualBasic;
using tman.src.Inventory;

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
        CGameCtnBlock referenceBlock,
        CGameCtnBlock block)
    {
        PlaceRelative(map, referenceBlock, block, new Vector3(0, 0, 0));
    }   
    public static void PlaceRelative(
        CGameCtnChallenge map,
        CGameCtnBlock referenceBlock,
        CGameCtnBlock block,
        Vector3 offset)
    {
        var positionOffset = referenceBlock.Direction switch
        {
            Direction.North => new(GridConstants.BlockSize, 0, 0),
            Direction.East => new(GridConstants.BlockSize, 0, GridConstants.BlockSize),
            Direction.South => new(0, 0, GridConstants.BlockSize),
            Direction.West => Vector3.Zero,
            _ => throw new ArgumentOutOfRangeException(nameof(referenceBlock))
        };
        var position = referenceBlock.AbsolutePositionInMap
            ?? new Vec3(
                referenceBlock.Coord.X * GridConstants.BlockSize + positionOffset.X,
                (referenceBlock.Coord.Y - GroundRow(map)) * GridConstants.VerticalBlockSize,
                referenceBlock.Coord.Z * GridConstants.BlockSize + positionOffset.Z);

        (float x, float z) = referenceBlock.Direction switch
        {
            Direction.North => (offset.X, offset.Z),
            Direction.East => (-offset.Z, offset.X),
            Direction.South => (-offset.X, -offset.Z),
            Direction.West => (offset.Z, -offset.X),
            _ => throw new ArgumentOutOfRangeException(nameof(referenceBlock))
        };
        float yaw = referenceBlock.Direction switch
        {
            Direction.North => 0f,
            Direction.East => -MathF.PI / 2f,
            Direction.South => MathF.PI,
            Direction.West => MathF.PI / 2f,
            _ => throw new ArgumentOutOfRangeException(nameof(referenceBlock))
        };
        block.Coord = referenceBlock.Coord;
        block.Direction = Direction.North;
        block.IsFree = true;
        block.YawPitchRoll = new Vec3(yaw, 0f, 0f);
        block.AbsolutePositionInMap = new Vec3(
            position.X + x,
            position.Y + offset.Y,
            position.Z + z);
        map.PlaceBlock(block);
    }
    public static void PlaceRelative(
        CGameCtnChallenge map,
        CGameCtnAnchoredObject referenceItem,
        CGameCtnAnchoredObject item)
    {
        PlaceRelative(map, referenceItem, item, new Vector3(0, 0, 0));
    }
    public static void PlaceRelative(
        CGameCtnChallenge map,
        CGameCtnAnchoredObject referenceItem,
        CGameCtnAnchoredObject item,
        Vector3 offset)
    {
        var rotatedOffset = referenceItem.YawPitchRoll.Y switch
        {
            var yaw when MathF.Abs(yaw) < 0.001f =>
                new Vector3(offset.X, offset.Y, offset.Z),
            var yaw when MathF.Abs(yaw + MathF.PI / 2f) < 0.001f =>
                new Vector3(offset.Z, offset.Y, -offset.X),
            var yaw when MathF.Abs(MathF.Abs(yaw) - MathF.PI) < 0.001f =>
                new Vector3(-offset.X, offset.Y, -offset.Z),
            var yaw when MathF.Abs(yaw - MathF.PI / 2f) < 0.001f =>
                new Vector3(-offset.Z, offset.Y, offset.X),
            _ => offset
        };
        var position = new Vec3(
            referenceItem.AbsolutePositionInMap.X + rotatedOffset.X,
            referenceItem.AbsolutePositionInMap.Y + rotatedOffset.Y,
            referenceItem.AbsolutePositionInMap.Z + rotatedOffset.Z);
        map.PlaceAnchoredObject(item.ItemModel, position, referenceItem.YawPitchRoll, Vec3.Zero);
    }
}
