using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.GameData;
using Tman.Core.Inventory;

namespace Tman.Core.Maps
{
    public static class MapPlacing
    {
        public static Placement PlaceRelative(this Map map, Placement source, Item model, Vec3 offset = default)
        {
            var position = source.Pose(map);
            position.Move(offset);
            return map.Place(model, position.Coords, position.YawPitchRoll);
        }

        public static Placement PlaceRelative(this Map map, Placement source, Block model, Vec3 offset = default)
        {
            var position = source.Pose(map);
            position.Move(offset);
            return map.PlaceBlock(new CGameCtnBlock
            {
                Name = model.Id,
                Coord = source.Cell,
                Direction = Direction.North,
                IsFree = true,
                AbsolutePositionInMap = position.Coords,
                YawPitchRoll = position.YawPitchRoll,
                WaypointSpecialProperty = MapWaypoints.Property(null, model.Waypoint)
            });
        }

        public static Placement Place(this Map map, Item model, Vec3 position, Vec3 yawPitchRoll = default)
        {
            var item = map.Gbx.PlaceAnchoredObject(model.Ident(map), position, yawPitchRoll, Vec3.Zero);
            item.WaypointSpecialProperty = MapWaypoints.Property(null, model.Waypoint);
            return new Placement(item);
        }

        public static Placement PlaceLike(this Map map, Placement source, Waypoint kind) => source.IsBlock
            ? map.PlaceBlock(Copy(source.Block!, MapReplacing.Twin(source.BlockModel, kind, source.Id), MapReplacing.Turn(source.Waypoint, kind), null, true))
            : map.PlaceLike(source, MapReplacing.Twin(source.ItemModel, kind, source.Id), MapReplacing.HalfTurn(source, kind));

        public static Placement PlaceLike(this Map map, Placement source, Block model) => map.PlaceBlock(Copy(source.Block!, model, DirectionOffset.None, null, false));

        public static Placement PlaceLike(this Map map, Placement source, Item model) => map.PlaceLike(source, model, 0);

        internal static Placement PlaceBlock(this Map map, CGameCtnBlock block)
        {
            map.Gbx.PlaceBlock(block);
            return new Placement(block);
        }

        internal static Placement PlaceLike(this Map map, Placement source, Item model, float yaw)
        {
            var item = source.Item!;
            var copy = map.Gbx.PlaceAnchoredObject(model.Ident(map), item.AbsolutePositionInMap, item.YawPitchRoll + new Vec3(yaw, 0, 0), item.PivotPosition);
            copy.BlockUnitCoord = item.BlockUnitCoord;
            copy.AnchorTreeId = item.AnchorTreeId;
            copy.Flags = item.Flags;
            copy.Scale = item.Scale;
            copy.PackDesc = item.PackDesc;
            copy.Color = item.Color;
            copy.WaypointSpecialProperty = MapWaypoints.Property(null, model.Waypoint);
            return new Placement(copy);
        }

        internal static CGameCtnBlock Copy(CGameCtnBlock block, Block model, DirectionOffset rotation, CGameWaypointSpecialProperty? property, bool sameFamily)
        {
            var copy = new CGameCtnBlock
            {
                Name = model.Id,
                Coord = block.Coord,
                Direction = block.Direction.Turn(rotation),
                Flags = sameFamily ? block.Flags : block.Flags & ~BlockFlags.AdditionalVariantMask,
                AbsolutePositionInMap = block.AbsolutePositionInMap,
                YawPitchRoll = block.YawPitchRoll,
                Color = block.Color,
                LightmapQuality = block.LightmapQuality
            };
            if (block.Skin is not null)
            {
                copy.Skin = block.Skin;
            }
            copy.WaypointSpecialProperty = MapWaypoints.Property(property, model.Waypoint);
            return copy;
        }
    }
}
