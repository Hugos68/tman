using System;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.GameData;
using tman.Inventory;

namespace tman.Maps
{
    public sealed class Placement
    {
        internal CGameCtnBlock? Block { get; }

        internal CGameCtnAnchoredObject? Item { get; }

        internal Placement(CGameCtnBlock block)
        {
            Block = block;
        }

        internal Placement(CGameCtnAnchoredObject item)
        {
            Item = item;
        }

        public bool IsBlock => Block is not null;

        public string Id => Block is not null ? Block.BlockModel.Id : Item!.ItemModel.Id;

        public uint Flags => Block is not null ? (uint)Block.Flags : (uint)Item!.Flags;

        public int Variant => Block is not null
            ? (Block.Flags & 0x3F) + 64 * ((Block.Flags >> 21) & 0x7F)
            : (Item!.Flags >> 8) & 0xFF;

        public Int3 Cell => Block?.Coord ?? Int3.Zero;

        public Direction Direction => Block?.Direction ?? Direction.North;

        public bool IsGround => Block?.IsGround ?? false;

        public bool IsFree => Block?.IsFree ?? false;

        internal CGameWaypointSpecialProperty? Property => Block is not null ? Block.WaypointSpecialProperty : Item!.WaypointSpecialProperty;

        public bool IsWaypoint => Property is not null;

        public Waypoint Waypoint => IsBlock
            ? BlockModel?.Waypoint ?? WaypointByName
            : ItemModel?.Waypoint ?? WaypointByName;

        public bool IsCheckpoint => IsWaypoint && Waypoint == Waypoint.Checkpoint;

        public bool IsStart => IsWaypoint && Waypoint is Waypoint.Start or Waypoint.StartFinish;

        public bool IsFinish => IsWaypoint && Waypoint is Waypoint.Finish or Waypoint.StartFinish;

        public Block? BlockModel => Block is not null ? Blocks.FromId(Block.BlockModel.Id) : null;

        public Item? ItemModel => Item is not null ? Items.FromId(Item.ItemModel.Id) ?? Custom.FromId(Item.ItemModel.Id) : null;

        public Position Pose(Map map)
        {
            if (Item is not null)
            {
                return new Position(Item.AbsolutePositionInMap, Item.YawPitchRoll);
            }
            if (Block!.IsFree)
            {
                return new Position(Block.AbsolutePositionInMap, Block.YawPitchRoll);
            }
            var size = BlockModel is { } model ? (model.SizeX, model.SizeZ) : (1, 1);
            var position = new Position(new Vec3(
                Block.Coord.X * MapConstants.CellSize,
                (Block.Coord.Y - map.GroundRow) * MapConstants.CellHeight,
                Block.Coord.Z * MapConstants.CellSize));
            position.AddPosition(new Position(DirectionOffsetOf(Block.Direction, size), DirectionRotationOf(Block.Direction)));
            return position;
        }

        private Waypoint WaypointByName =>
            !IsWaypoint ? Waypoint.None
            : Id.Contains("Checkpoint", StringComparison.Ordinal) ? Waypoint.Checkpoint
            : Id.Contains("Finish", StringComparison.Ordinal) ? Waypoint.Finish
            : Id.Contains("Start", StringComparison.Ordinal) ? Waypoint.Start
            : Waypoint.None;

        private static Vec3 DirectionOffsetOf(Direction direction, (int X, int Z) size) =>
            direction switch
            {
                Direction.North => Vec3.Zero,
                Direction.East => new(size.Z * MapConstants.CellSize, 0, 0),
                Direction.South => new(size.X * MapConstants.CellSize, 0, size.Z * MapConstants.CellSize),
                Direction.West => new(0, 0, size.X * MapConstants.CellSize),
                _ => throw new ArgumentOutOfRangeException(nameof(direction))
            };

        private static Vec3 DirectionRotationOf(Direction direction) =>
            direction switch
            {
                Direction.North => Vec3.Zero,
                Direction.East => new(-MathF.PI / 2, 0, 0),
                Direction.South => new(MathF.PI, 0, 0),
                Direction.West => new(MathF.PI / 2, 0, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(direction))
            };
    }
}
