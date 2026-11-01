using System;
using GBX.NET;
using GBX.NET.Engines.Game;
using Tman.Core.Maps.Placements;
using Tman.Core.Maps.Geometry;

namespace Tman.Core.Maps
{
    public static class MapMoving
    {
        public static void Free(this Map map, Placement placement)
        {
            var block = placement.Block!;
            if (block.IsFree)
            {
                return;
            }
            var pose = placement.Pose(map);
            map.Gbx.TryCreateChunk<CGameCtnChallenge.Chunk0304305F>(out _);
            block.IsFree = true;
            block.IsGround = false;
            block.Coord = new Int3(-1, 0, -1);
            block.Direction = Direction.North;
            block.AbsolutePositionInMap = pose.Coords;
            block.YawPitchRoll = pose.YawPitchRoll;
        }

        public static void Nudge(Placement placement, Vec3 offset, Vec3 yawPitchRoll)
        {
            if (placement.Block is { } block)
            {
                block.AbsolutePositionInMap += offset;
                block.YawPitchRoll += yawPitchRoll;
                return;
            }
            placement.Item!.AbsolutePositionInMap += offset;
            placement.Item.YawPitchRoll += yawPitchRoll;
        }

        public static void Translate(this Map map, Placement placement, Vec3 offset)
        {
            if (placement.Block is { IsFree: false } block && offset.X % MapConstants.CellSize == 0 && offset.Y % MapConstants.CellHeight == 0 && offset.Z % MapConstants.CellSize == 0)
            {
                var coord = block.Coord + new Int3((int)(offset.X / MapConstants.CellSize), (int)(offset.Y / MapConstants.CellHeight), (int)(offset.Z / MapConstants.CellSize));
                if (coord.X < 0 || coord.Y < 0 || coord.Z < 0 || coord.X >= map.Size.X || coord.Y >= map.Size.Y || coord.Z >= map.Size.Z)
                {
                    throw new InvalidOperationException($"{placement.Id} at {block.Coord} cannot move by {offset}: outside the map");
                }
                block.Coord = coord;
                return;
            }
            if (placement.IsBlock)
            {
                map.Free(placement);
            }
            Nudge(placement, offset, Vec3.Zero);
        }
    }
}
