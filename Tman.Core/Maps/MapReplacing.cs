using System;
using GBX.NET;
using Tman.Core.Inventory;
using Tman.Core.Inventory.Builders;

namespace Tman.Core.Maps
{
    public static class MapReplacing
    {
        public static Placement Replace(this Map map, Placement source, Waypoint kind)
        {
            if (source.IsBlock)
            {
                return map.Replace(source, Twin(source.BlockModel, kind, source.Id), Turn(source.Waypoint, kind), true);
            }
            var item = source.Item!;
            item.YawPitchRoll += new Vec3(HalfTurn(source, kind), 0, 0);
            return map.Replace(source, Twin(source.ItemModel, kind, source.Id));
        }

        public static Placement Replace(this Map map, Placement source, Block model) => map.Replace(source, model, DirectionOffset.None, true);

        public static Placement Replace(this Map map, Placement source, Block model, DirectionOffset rotation) => map.Replace(source, model, rotation, false);

        public static Placement Replace(this Map map, Placement source, Item model)
        {
            var item = source.Item!;
            item.ItemModel = model.Ident(map);
            item.WaypointSpecialProperty = MapWaypoints.Property(item.WaypointSpecialProperty, model.Waypoint);
            return source;
        }

        public static Placement Replace(this Map map, Placement source, SurfaceBlock model)
        {
            var block = source.Block!;
            var file = new SurfaceBlockFile(model, block.IsGround, BlockFlags.AdditionalVariant(block.Flags), map.Collection);
            map.EmbedSurfaceBlock(file, SurfaceBlockCache.GetOrBuild(file));
            block.Name = file.RecordName;
            block.Flags |= BlockFlags.CustomBlock;
            block.Author = file.RecordAuthor;
            block.Skin = null;
            return source;
        }

        private static Placement Replace(this Map map, Placement source, Block model, DirectionOffset rotation, bool sameFamily)
        {
            var block = source.Block!;
            map.Gbx.RemoveBlock(block);
            return map.PlaceBlock(MapPlacing.Copy(block, model, rotation, block.WaypointSpecialProperty, sameFamily));
        }

        internal static Block Twin(Block? model, Waypoint kind, string id) =>
            model?.As(kind) ?? throw new InvalidOperationException($"{id} has no {kind} twin");

        internal static Item Twin(Item? model, Waypoint kind, string id) =>
            model?.As(kind) ?? throw new InvalidOperationException($"{id} has no {kind} twin");

        internal static float HalfTurn(Placement source, Waypoint kind) => Items.HalfTurned.Contains((source.Id, kind)) ? MathF.PI : 0;

        internal static DirectionOffset Turn(Waypoint from, Waypoint to) =>
            (from, to) is (Waypoint.Start, Waypoint.Finish) or (Waypoint.Finish, Waypoint.Start) ? DirectionOffset.Half : DirectionOffset.None;
    }
}
