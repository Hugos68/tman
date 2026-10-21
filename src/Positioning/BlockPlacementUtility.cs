
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
        CGameCtnBlock referenceBlock,
        CGameCtnBlock block,
        Vector3 offset)
    {
        Vec3 position = referenceBlock.AbsolutePositionInMap
            ?? new Vec3(
                referenceBlock.Coord.X * GridConstants.BlockSize + GridConstants.BlockSize,
                (referenceBlock.Coord.Y - GroundRow(map)) * GridConstants.VerticalBlockSize,
                referenceBlock.Coord.Z * GridConstants.BlockSize);

        (float X, float Z) = referenceBlock.Direction switch
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
        block.IsGround = referenceBlock.IsGround;
        block.Variant = referenceBlock.Variant;
        block.SubVariant = referenceBlock.SubVariant;
        block.IsFree = true;
        block.YawPitchRoll = new Vec3(yaw, 0f, 0f);
        block.AbsolutePositionInMap = new Vec3(
            position.X + X,
            position.Y + offset.Y,
            position.Z + Z);

        map.PlaceBlock(block);
    }
    public static void PlaceRelative(
        CGameCtnChallenge map,
        CGameCtnAnchoredObject referenceItem,
        CGameCtnAnchoredObject item,
        Vector3 offset)
    {
        var position = referenceItem.AbsolutePositionInMap;
        var rotation = referenceItem.YawPitchRoll;

        var rotatedOffset = rotation.Y switch
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

        map.PlaceAnchoredObject(
            item.ItemModel,
            new Vec3(
                position.X + rotatedOffset.X,
                position.Y + rotatedOffset.Y,
                position.Z + rotatedOffset.Z),
            rotation,
            new Vec3(1, 1, 1));
    }
}
