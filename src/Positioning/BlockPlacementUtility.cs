using System;
using System.Numerics;
using GBX.NET;
using GBX.NET.Engines.Game;

namespace tman.src.Positioning;

public static class BlockPlacementUtility
{
    private const float BlockSize = 32f;
    private const float VerticalBlockSize = 8f;

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
        Vector3 offset)
    {
        (float X, float Z) rotatedOffset = reference.Direction switch
        {
            Direction.North => (offset.X, offset.Z),
            Direction.East => (-offset.Z, offset.X),
            Direction.South => (-offset.X, -offset.Z),
            Direction.West => (offset.Z, -offset.X),
            _ => throw new ArgumentOutOfRangeException(nameof(reference))
        };

        Vec3 position = reference.AbsolutePositionInMap
            ?? new Vec3(
                reference.Coord.X * BlockSize + BlockSize / 2,
                (reference.Coord.Y - GroundRow(map)) * VerticalBlockSize,
                reference.Coord.Z * BlockSize + BlockSize / 2);

        block.Coord = reference.Coord;
        block.Direction = reference.Direction;
        block.YawPitchRoll = reference.YawPitchRoll;
        block.IsFree = true;
        block.AbsolutePositionInMap = new Vec3(
            position.X + rotatedOffset.X,
            position.Y + offset.Y,
            position.Z + rotatedOffset.Z);
        map.PlaceBlock(block);
    }
}