using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.GameData;
using Tman.Core.Catalog;
using Tman.Core.Maps.Geometry;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Maps
{
    public sealed partial class Map
    {
        internal Placement PlaceRelative(Placement source, Item model, Vec3 offset = default)
        {
            var position = source.Pose(this);
            position.Move(offset);
            return Place(model, position.Coords, position.YawPitchRoll);
        }

        internal Placement PlaceRelative(Placement source, Block model, Vec3 offset = default)
        {
            var position = source.Pose(this);
            position.Move(offset);
            return PlaceBlock(new CGameCtnBlock
            {
                Name = model.Id,
                Coord = source.Cell,
                Direction = Direction.North,
                IsFree = true,
                AbsolutePositionInMap = position.Coords,
                YawPitchRoll = position.YawPitchRoll,
                WaypointSpecialProperty = Property(null, model.Waypoint)
            });
        }

        internal Placement Place(Item model, Vec3 position, Vec3 yawPitchRoll = default)
        {
            var item = Gbx.PlaceAnchoredObject(model.Ident(this), position, yawPitchRoll, Vec3.Zero);
            item.WaypointSpecialProperty = Property(null, model.Waypoint);
            return new Placement(item);
        }

        internal Placement PlaceLike(Placement source, Waypoint kind) => source.IsBlock
            ? PlaceBlock(Copy(source.Block!, Twin(source.BlockModel, kind, source.Id), Turn(source.Waypoint, kind), null, true))
            : PlaceLike(source, Twin(source.ItemModel, kind, source.Id), HalfTurn(source, kind));

        internal Placement PlaceLike(Placement source, Block model) => PlaceBlock(Copy(source.Block!, model, DirectionOffset.None, null, false));

        internal Placement PlaceLike(Placement source, Item model) => PlaceLike(source, model, 0);

        internal Placement PlaceBlock(CGameCtnBlock block)
        {
            Gbx.PlaceBlock(block);
            return new Placement(block);
        }

        internal Placement PlaceLike(Placement source, Item model, float yaw)
        {
            var item = source.Item!;
            var copy = Gbx.PlaceAnchoredObject(model.Ident(this), item.AbsolutePositionInMap, item.YawPitchRoll + new Vec3(yaw, 0, 0), item.PivotPosition);
            copy.BlockUnitCoord = item.BlockUnitCoord;
            copy.AnchorTreeId = item.AnchorTreeId;
            copy.Flags = item.Flags;
            copy.Scale = item.Scale;
            copy.PackDesc = item.PackDesc;
            copy.Color = item.Color;
            copy.WaypointSpecialProperty = Property(null, model.Waypoint);
            return new Placement(copy);
        }

        private static CGameCtnBlock Copy(CGameCtnBlock block, Block model, DirectionOffset rotation, CGameWaypointSpecialProperty? property, bool sameFamily)
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
            copy.WaypointSpecialProperty = Property(property, model.Waypoint);
            return copy;
        }
    }
}
