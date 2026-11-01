using System;
using GBX.NET;
using Tman.Core.Catalog;
using Tman.Core.Surfaces;
using Tman.Core.Surfaces.Building;
using Tman.Core.Maps.Geometry;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Maps
{
    public sealed partial class Map
    {
        internal Placement Replace(Placement source, Waypoint kind)
        {
            if (source.IsBlock)
            {
                return Replace(source, Twin(source.BlockModel, kind, source.Id), Turn(source.Waypoint, kind), true);
            }
            var item = source.Item!;
            item.YawPitchRoll += new Vec3(HalfTurn(source, kind), 0, 0);
            return Replace(source, Twin(source.ItemModel, kind, source.Id));
        }

        internal Placement Replace(Placement source, Block model) => Replace(source, model, DirectionOffset.None, true);

        internal Placement Replace(Placement source, Block model, DirectionOffset rotation) => Replace(source, model, rotation, false);

        internal Placement Replace(Placement source, Item model)
        {
            var item = source.Item!;
            item.ItemModel = model.Ident(this);
            item.WaypointSpecialProperty = Property(item.WaypointSpecialProperty, model.Waypoint);
            return source;
        }

        internal Placement Replace(Placement source, SurfaceBlock model)
        {
            var block = source.Block!;
            var file = new SurfaceBlockFile(model, block.IsGround, BlockFlags.AdditionalVariant(block.Flags), Collection);
            EmbedSurfaceBlock(file, SurfaceBlockCache.GetOrBuild(file));
            block.Name = file.RecordName;
            block.Flags |= BlockFlags.CustomBlock;
            block.Author = file.RecordAuthor;
            block.Skin = null;
            return source;
        }

        private Placement Replace(Placement source, Block model, DirectionOffset rotation, bool sameFamily)
        {
            var block = source.Block!;
            Challenge.RemoveBlock(block);
            return PlaceBlock(Copy(block, model, rotation, block.WaypointSpecialProperty, sameFamily));
        }

        private static Block Twin(Block? model, Waypoint kind, string id) =>
            model?.As(kind) ?? throw new InvalidOperationException($"{id} has no {kind} twin");

        private static Item Twin(Item? model, Waypoint kind, string id) =>
            model?.As(kind) ?? throw new InvalidOperationException($"{id} has no {kind} twin");

        private static float HalfTurn(Placement source, Waypoint kind) => Items.HalfTurned.Contains((source.Id, kind)) ? MathF.PI : 0;

        private static DirectionOffset Turn(Waypoint from, Waypoint to) =>
            (from, to) is (Waypoint.Start, Waypoint.Finish) or (Waypoint.Finish, Waypoint.Start) ? DirectionOffset.Half : DirectionOffset.None;
    }
}
