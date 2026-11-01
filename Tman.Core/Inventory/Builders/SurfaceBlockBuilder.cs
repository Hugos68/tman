using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.GameData;
using GBX.NET.Engines.Plug;
using Tman.Paks;

namespace Tman.Core.Inventory.Builders
{
    public static class SurfaceBlockBuilder
    {
        private const string Template = "SurfaceBlock.Block.Gbx";

        private const string BlockInfoClassic = @"GameCtnBlockInfo\GameCtnBlockInfoClassic";

        private const string BlockInfoExtension = ".EDClassic.Gbx";

        private const string StadiumFolder = "Stadium";

        private const byte StaticObjectFlag = 1;

        private const byte TriggerFlag = 2;

        private const byte SpawnFlag = 4;

        public static byte[] Build(SurfaceBlockFile file)
        {
            try
            {
                return BuildUnchecked(file);
            }
            catch (Exception exception) when (exception is not InvalidOperationException)
            {
                throw new InvalidOperationException($"Cannot build {file.FileName}: {exception.Message}", exception);
            }
        }

        private static byte[] BuildUnchecked(SurfaceBlockFile file)
        {
            var store = GamePaks.Store;
            var (infoPath, info) = BlockInfo(store, file.Block.Archetype, file.Collection);
            var variant = Variant(info, file.Ground, file.AdditionalVariant) ?? throw new InvalidOperationException($"{file.Block.Archetype.Id} has no {(file.Ground ? "ground" : "air")} variant {file.AdditionalVariant}");
            var ancestor = store.Open(infoPath)!.RefTable!.AncestorLevel;
            var parts = new List<PrefabMerge.Part>();
            foreach (var mobil in (variant.Mobils ?? []).SelectMany(row => row))
            {
                var prefab = PakStore.Logical(infoPath, ancestor, mobil.PrefabFidFile!.FilePath);
                parts.AddRange(PrefabMerge.Collect(store, prefab, mobil.HasGeomTransformation ? mobil.GeomTranslation : Vec3.Zero, new Quat(0, 0, 0, 1)));
            }
            var family = RoadFamily.Of(file.Block.Archetype) ?? throw new InvalidOperationException($"{file.Block.Archetype.Id} is not a road kind");
            var model = SurfaceBlockMesh.Merge(parts, family, SurfaceTarget.Of(file.Block.Surface), MaterialDressing.For(file.Collection, infoPath, info, ancestor));
            return Shell(file, variant, model, infoPath, ancestor, store);
        }

        private static byte[] Shell(SurfaceBlockFile file, CGameCtnBlockInfoVariant variant, CPlugStaticObjectModel model, string infoPath, int ancestor, PakStore store)
        {
            var item = GBX.NET.Gbx.Parse<CGameItemModel>(Assets.Data(Template));
            var collection = CollectionIds.Of(file.Collection);
            item.Node.Ident = new Ident(file.Ident, collection, file.RecordAuthor);
            item.Node.Name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file.FileName));
            var edition = (CGameBlockItem)item.Node.EntityModelEdition!;
            edition.ArchetypeBlockInfoId = file.Block.Archetype.Id;
            edition.ArchetypeBlockInfoCollectionId = collection;
            var properties = new CGameBlockItem.MobilProperties { Flags = StaticObjectFlag, StaticObject = model };
            if (variant.WaypointTriggerSolidFile is { } triggerFile && store.Open(PakStore.Logical(infoPath, ancestor, triggerFile.FilePath))?.Node is CPlugSurface trigger)
            {
                properties.Shape = trigger;
                properties.Flags |= TriggerFlag;
            }
            if (variant.SpawnTrans is { } spawn && spawn != Vec3.Zero)
            {
                properties.U01 = new BoxAligned(spawn.X, spawn.Y, spawn.Z, 0, 0, 0);
                properties.Flags |= SpawnFlag;
            }
            edition.CustomizedVariants[0] = new CGameBlockItem.Mobil { Id = SurfaceBlockKey.Of(file.Ground, file.AdditionalVariant), Properties = properties };
            using var stream = new MemoryStream();
            item.Save(stream);
            return stream.ToArray();
        }

        private static readonly Lazy<Dictionary<(string Environment, string Kind), string>> BlockInfoIndex = new(() =>
        {
            var index = new Dictionary<(string, string), string>();
            foreach (var entry in GamePaks.Store.Entries)
            {
                var segments = entry.Split('\\');
                if (segments.Length > 3 && segments[1] + "\\" + segments[2] == BlockInfoClassic && entry.EndsWith(BlockInfoExtension, StringComparison.OrdinalIgnoreCase))
                {
                    index.TryAdd((segments[0], segments[^1][..^BlockInfoExtension.Length]), entry);
                }
            }
            return index;
        });

        private static (string Path, CGameCtnBlockInfo Info) BlockInfo(PakStore store, Block archetype, Collections collection)
        {
            foreach (var environment in collection == Collections.Stadium ? [StadiumFolder] : new[] { collection.ToString(), StadiumFolder })
            {
                if (BlockInfoIndex.Value.TryGetValue((environment, archetype.Id), out var path) && store.Open(path)?.Node is CGameCtnBlockInfo info)
                {
                    return (path, info);
                }
            }
            throw new InvalidOperationException($"No block definition for {archetype.Id} in {collection}");
        }

        private static CGameCtnBlockInfoVariant? Variant(CGameCtnBlockInfo info, bool ground, int additional) => (ground, additional) switch
        {
            (true, 0) => info.VariantBaseGround,
            (false, 0) => info.VariantBaseAir,
            (true, _) => info.AdditionalVariantsGround?.ElementAtOrDefault(additional - 1),
            (false, _) => info.AdditionalVariantsAir?.ElementAtOrDefault(additional - 1)
        };
    }
}
