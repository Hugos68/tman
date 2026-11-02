using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GBX.NET;
using GBX.NET.Components;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.GameData;
using GBX.NET.Engines.Plug;
using Tman.Core.Catalog;
using Tman.Core.Game;
using Microsoft.Extensions.Logging;
using Tman.Paks;

namespace Tman.Core.Surfaces.Building
{
    internal sealed class SurfaceBlockBuilder(IPakStore paks, IAssets assets, GameMaterials materials, SurfaceBlockMesh mesh, ILogger<SurfaceBlockBuilder> logger)
    {
        private const string Template = "SurfaceBlock.Block.Gbx";

        private const string BlockInfoFolder = @"GameCtnBlockInfo\GameCtnBlockInfoClassic";

        private const string BlockInfoExtension = ".EDClassic.Gbx";

        private const int BlockInfoDepth = 3;

        private const string StadiumFolder = "Stadium";

        private const byte StaticObjectFlag = 1;

        private const byte TriggerFlag = 2;

        private const byte SpawnFlag = 4;

        private readonly Lazy<BlockInfoIndex> blockInfos = new(() => BlockInfoIndex.Of(paks));

        public byte[] Build(SurfaceBlockFile file)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var bytes = BuildUnchecked(file);
                logger.LogDebug("Built {Block} ({Size} bytes) in {Elapsed} ms", file.Name, bytes.Length, stopwatch.ElapsedMilliseconds);
                return bytes;
            }
            catch (Exception exception) when (exception is not InvalidOperationException)
            {
                throw new InvalidOperationException($"Cannot build {file.FileName}: {exception.Message}", exception);
            }
        }

        private byte[] BuildUnchecked(SurfaceBlockFile file)
        {
            var (infoPath, info) = BlockInfo(file.Block.Archetype, file.Collection);
            var variant = Variant(info, file.Ground, file.AdditionalVariant) ?? throw new InvalidOperationException($"{file.Block.Archetype.Id} has no {(file.Ground ? "ground" : "air")} variant {file.AdditionalVariant}");
            var ancestor = paks.Open(infoPath)!.RefTable!.AncestorLevel;
            var parts = new List<PrefabParts.Part>();
            foreach (var mobil in (variant.Mobils ?? []).Select(variations => variations[0]))
            {
                var prefab = PakStore.Logical(infoPath, ancestor, mobil.PrefabFidFile!.FilePath);
                var position = mobil.HasGeomTransformation ? mobil.GeomTranslation : Vec3.Zero;
                var rotation = mobil.HasGeomTransformation ? Transforms.FromEulerDegrees(mobil.GeomRotation) : Quat.Identity;
                parts.AddRange(PrefabParts.Collect(paks, prefab, position, rotation));
            }
            var family = DeckFamily.Of(file.Block.Archetype) ?? throw new InvalidOperationException($"{file.Block.Archetype.Id} is not a road kind");
            var model = mesh.Merge(parts, family, SurfaceTarget.Of(file.Block.Surface, family), MaterialDressing.For(paks, materials, file.Collection, infoPath, info, ancestor));
            return Shell(file, info, variant, model, infoPath, ancestor);
        }

        private byte[] Shell(SurfaceBlockFile file, CGameCtnBlockInfo info, CGameCtnBlockInfoVariant variant, CPlugStaticObjectModel model, string infoPath, int ancestor)
        {
            using var template = assets.Open(Assets.Data(Template));
            var item = Gbx.Parse<CGameItemModel>(template);
            var collection = CollectionIds.Of(file.Collection);
            item.Node.Ident = new Ident(file.Ident, collection, file.RecordAuthor);
            item.Node.Name = file.Name;
            item.Node.PageName = file.Block.Archetype.Id;
            item.Node.Description = file.Block.Name;
            item.Node.IconWebP = info.IconWebP;
            var edition = (CGameBlockItem)item.Node.EntityModelEdition!;
            edition.ArchetypeBlockInfoId = file.Block.Archetype.Id;
            edition.ArchetypeBlockInfoCollectionId = collection;
            var properties = new CGameBlockItem.MobilProperties { Flags = StaticObjectFlag, StaticObject = model };
            if (info.WayPointType != CGameCtnBlockInfo.EWayPointType.None)
            {
                if (TriggerShapeFile(variant) is { } triggerFile && paks.Open(PakStore.Logical(infoPath, ancestor, triggerFile.FilePath)) is { Node: CPlugSurface trigger } triggerGbx)
                {
                    properties.Shape = mesh.TriggerShape(trigger, triggerGbx, PakStore.Logical(infoPath, ancestor, triggerFile.FilePath));
                    properties.Flags |= TriggerFlag;
                }
                if (SpawnBox(variant) is { } spawn && spawn != default)
                {
                    properties.U01 = spawn;
                    properties.Flags |= SpawnFlag;
                }
            }
            edition.CustomizedVariants[0] = new CGameBlockItem.Mobil { Id = SurfaceBlockKey.Of(file.Ground, file.AdditionalVariant), Properties = properties };
            using var stream = new MemoryStream();
            item.Save(stream);
            return stream.ToArray();
        }

        private sealed record BlockInfoIndex(Dictionary<string, string> ByFileName, Dictionary<string, string> ByRenamedId)
        {
            public static BlockInfoIndex Of(IPakStore paks)
            {
                var byFileName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var entry in paks.Entries)
                {
                    var segments = entry.Split(PakPath.Separator);
                    if (segments.Length > BlockInfoDepth && PakPath.Join(segments[1], segments[2]) == BlockInfoFolder && entry.EndsWith(BlockInfoExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        byFileName.TryAdd(PakPath.Join(segments[0], segments[^1][..^BlockInfoExtension.Length]), entry);
                    }
                }
                var byRenamedId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var (key, path) in byFileName)
                {
                    if (paks.Open(path)?.Node is CGameCtnBlockInfo info && !string.Equals(info.Ident.Id, PakPath.Name(key), StringComparison.OrdinalIgnoreCase))
                    {
                        byRenamedId.TryAdd(PakPath.Join(PakPath.Environment(key), info.Ident.Id), path);
                    }
                }
                return new BlockInfoIndex(byFileName, byRenamedId);
            }

            public string? Path(string key) => ByFileName.TryGetValue(key, out var path) || ByRenamedId.TryGetValue(key, out path) ? path : null;
        }

        private (string Path, CGameCtnBlockInfo Info) BlockInfo(Block archetype, Collections collection)
        {
            foreach (var environment in collection == Collections.Stadium ? [StadiumFolder] : new[] { collection.ToString(), StadiumFolder })
            {
                var key = PakPath.Join(environment, archetype.Id);
                if (blockInfos.Value.Path(key) is { } path && paks.Open(path)?.Node is CGameCtnBlockInfo info)
                {
                    return (path, info);
                }
            }
            throw new InvalidOperationException($"No block definition for {archetype.Id} in {collection}");
        }

        private static GbxRefTableFile? TriggerShapeFile(CGameCtnBlockInfoVariant variant) =>
            variant.Chunks.OfType<CGameCtnBlockInfoVariant.Chunk0315B006>().FirstOrDefault()?.U03File;

        private static BoxAligned? SpawnBox(CGameCtnBlockInfoVariant variant) =>
            variant.Chunks.OfType<CGameCtnBlockInfoVariant.Chunk0315B008>().FirstOrDefault()?.U02;

        private static CGameCtnBlockInfoVariant? Variant(CGameCtnBlockInfo info, bool ground, int additional) => (ground, additional) switch
        {
            (true, 0) => info.VariantBaseGround,
            (false, 0) => info.VariantBaseAir,
            (true, _) => info.AdditionalVariantsGround?.ElementAtOrDefault(additional - 1),
            (false, _) => info.AdditionalVariantsAir?.ElementAtOrDefault(additional - 1)
        };
    }
}
