using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GBX.NET;
using GBX.NET.Components;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.GameData;
using GBX.NET.Engines.Meta;
using GBX.NET.Engines.MwFoundations;
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

        private const string InvisibleTemplate = "InvisibleItem.Item.Gbx";

        private const string CollisionMaterialTemplate = @"Stadium\Media\Modifier\Turbo2\Collision.Material.Gbx";

        private const string TerrainModifierExtension = ".TerrainModifier.Gbx";

        private const string GameSkinExtension = ".GameSkin.gbx";

        private const string DynaObjectExtension = ".DynaObject.Gbx";

        private const string PillarFolder = @"GameCtnBlockInfo\GameCtnBlockInfoPillar";

        private const string ClipFolder = @"GameCtnBlockInfo\GameCtnBlockInfoClip";

        private const string FlatFolder = @"GameCtnBlockInfo\GameCtnBlockInfoFlat";

        private const string FrontierFolder = @"GameCtnBlockInfo\GameCtnBlockInfoFrontier";

        private const string TransitionFolder = @"GameCtnBlockInfo\GameCtnBlockInfoTransition";

        private static readonly string[] IndexedFolders = [PillarFolder, ClipFolder, FlatFolder, FrontierFolder, TransitionFolder];

        private static readonly string[] BlockInfoFolders = [@"GameCtnBlockInfo\GameCtnBlockInfoClassic", .. IndexedFolders];

        private const char ExtensionStart = '.';

        private static readonly string[] BlockInfoExtensions = [".EDClassic.Gbx", ".EDClip.Gbx", ".EDHorizontalClip.Gbx", ".EDVerticalClip.Gbx", ".EDFlat.Gbx", ".EDFrontier.Gbx", ".EDTransition.Gbx"];

        private const int BlockInfoDepth = 3;

        private const string StadiumFolder = "Stadium";

        private const string ItemsFolder = "Items";

        private const string ItemExtension = ".Item.Gbx";

        private static readonly string[] EntityExtensions = [".Prefab.Gbx", ".StaticObject.Gbx", ".VegetTreeModel.Gbx"];

        private static readonly string[] SilentEntityExtensions = [".DynaObject.Gbx", ".VegetTreeModel.Gbx", ".FxSys.Gbx"];

        private const byte StaticObjectFlag = 1;

        private const byte TriggerFlag = 2;

        private const byte SpawnFlag = 4;

        private readonly Lazy<BlockInfoIndex> blockInfos = new(() => BlockInfoIndex.Of(paks));

        private readonly Lazy<Dictionary<string, string>> items = new(() => ItemIndex(paks));

        private readonly Lazy<Dictionary<string, string>> filesByName = new(() => FileIndex(paks));

        public byte[]? Build(SurfaceBlockFile file)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var bytes = BuildUnchecked(file);
                logger.LogDebug("Built {Block} ({Size} bytes) in {Elapsed} ms", file.Name, bytes?.Length ?? 0, stopwatch.ElapsedMilliseconds);
                return bytes;
            }
            catch (Exception exception) when (exception is not InvalidOperationException)
            {
                throw new InvalidOperationException($"Cannot build {file.FileName}: {exception.Message}", exception);
            }
        }

        public byte[]? BuildCustomItem(SurfaceBlockFile file, byte[] item)
        {
            try
            {
                using var stream = new MemoryStream(item);
                var gbx = Gbx.Parse<CGameItemModel>(stream);
                if (gbx.Node.EntityModel is not CGameCommonItemEntityModel { StaticObject: CPlugStaticObjectModel model })
                {
                    return null;
                }
                var parts = new List<PrefabParts.Part> { new(model, gbx, file.Ident, Vec3.Zero, Quat.Identity) };
                return mesh.Collision(parts, null) is { Shape: not null } collision ? InvisibleShell(file, collision) : [];
            }
            catch (Exception exception) when (exception is not InvalidOperationException)
            {
                throw new InvalidOperationException($"Cannot build {file.FileName}: {exception.Message}", exception);
            }
        }

        private byte[]? BuildUnchecked(SurfaceBlockFile file)
        {
            if (file.Item)
            {
                return InvisibleItem(file);
            }
            if (BlockInfo(file.Block.Archetype, file.Collection) is not var (infoPath, info))
            {
                return file.Block.Surface == Surface.Invisible ? null : throw new InvalidOperationException($"No block definition for {file.Block.Archetype.Id} in {file.Collection}");
            }
            var indexed = IndexedFolders.Any(folder => infoPath.Contains(folder, StringComparison.OrdinalIgnoreCase));
            var variant = Variant(info, file.Ground, indexed ? 0 : file.AdditionalVariant) ?? throw new InvalidOperationException($"{file.Block.Archetype.Id} has no {(file.Ground ? "ground" : "air")} variant {file.AdditionalVariant}");
            var ancestor = paks.Open(infoPath)!.RefTable?.AncestorLevel ?? 0;
            var parts = new List<PrefabParts.Part>();
            var mobils = (variant.Mobils ?? []).Select(variations => variations.FirstOrDefault());
            foreach (var mobil in (indexed ? mobils.Skip(file.BaseVariant).Take(1) : mobils).OfType<CGameCtnBlockInfoMobil>().Where(mobil => mobil.PrefabFidFile is not null))
            {
                var prefab = PakStore.Logical(infoPath, ancestor, mobil.PrefabFidFile!.FilePath);
                var position = mobil.HasGeomTransformation ? mobil.GeomTranslation : Vec3.Zero;
                var rotation = mobil.HasGeomTransformation ? Transforms.FromEulerDegrees(mobil.GeomRotation) : Quat.Identity;
                parts.AddRange(PrefabParts.Collect(paks, prefab, position, rotation));
            }
            if (file.Block.Surface != Surface.Invisible)
            {
                return Shell(file, info, variant, Merge(file, parts, infoPath, info, ancestor), infoPath, ancestor);
            }
            return mesh.Collision(parts, MaterialDressing.For(paks, materials, file.Collection, infoPath, info, ancestor)) is { Shape: not null } collision ? InvisibleShell(file, collision) : [];
        }

        private byte[]? InvisibleItem(SurfaceBlockFile file)
        {
            if (ItemEntity(file.Block.Archetype.Id, file.Collection, file.BaseVariant) is not var (entity, dressing, skin))
            {
                return null;
            }
            if (Kinematic(entity, dressing, skin) is { } moving)
            {
                return InvisiblePrefabShell(file, moving);
            }
            if (Animated(entity))
            {
                return null;
            }
            var parts = PrefabParts.Collect(paks, entity, Vec3.Zero, Quat.Identity);
            var collision = mesh.Collision(parts, dressing) is { Shape: not null } model ? model : null;
            var triggers = SpecialTriggers(entity, dressing);
            if (triggers.Count > 0)
            {
                var prefab = (CPlugPrefab)paks.Open(entity)!.Node!.DeepClone();
                prefab.Ents = [.. (collision is null ? [] : new[] { new CPlugPrefab.EntRef { Model = collision, Position = Vec3.Zero, Rotation = Quat.Identity, U01 = "" } }), .. triggers];
                return InvisiblePrefabShell(file, prefab);
            }
            return collision is not null ? InvisibleShell(file, collision) : [];
        }

        private List<CPlugPrefab.EntRef> SpecialTriggers(string path, MaterialDressing dressing)
        {
            var triggers = new List<CPlugPrefab.EntRef>();
            if (paks.Open(path) is not { Node: CPlugPrefab prefab } gbx)
            {
                return triggers;
            }
            foreach (var entity in prefab.Ents)
            {
                if (entity.Model is not NPlugTrigger_SSpecial special)
                {
                    continue;
                }
                var (shape, owner, ownerPath) = special.TriggerShape is { } inline ? (inline, gbx, path)
                    : special.TriggerShapeFile is { } file && PakStore.Logical(path, gbx.RefTable!.AncestorLevel, file.FilePath) is var shapePath && paks.Open(shapePath) is { Node: CPlugSurface referenced } shapeGbx ? (referenced, shapeGbx, shapePath)
                    : (null, gbx, path);
                if (shape is null)
                {
                    continue;
                }
                var trigger = (NPlugTrigger_SSpecial)special.DeepClone();
                trigger.TriggerShape = mesh.InlineSurface(shape, owner, ownerPath, dressing, CollisionMaterial());
                trigger.TriggerShapeFile = null;
                triggers.Add(new CPlugPrefab.EntRef { Model = trigger, Position = entity.Position, Rotation = entity.Rotation, U01 = entity.U01 });
            }
            return triggers;
        }

        private CPlugPrefab? Kinematic(string path, MaterialDressing dressing, IReadOnlyDictionary<string, string> skin)
        {
            if (paks.Open(path) is not { Node: CPlugPrefab prefab } gbx || !prefab.Ents.Any(entity => entity.ModelFile?.FilePath.EndsWith(DynaObjectExtension, StringComparison.OrdinalIgnoreCase) == true))
            {
                return null;
            }
            var ents = new List<CPlugPrefab.EntRef>();
            foreach (var entity in prefab.Ents)
            {
                if (entity.ModelFile is not { } file)
                {
                    return null;
                }
                var logical = PakStore.Logical(path, gbx.RefTable!.AncestorLevel, file.FilePath);
                logical = skin.GetValueOrDefault(logical, logical);
                CMwNod? model = paks.Open(logical) is { } node ? node.Node switch
                {
                    CPlugDynaObjectModel dyna => InvisibleDyna(dyna, node, logical, dressing),
                    NPlugDyna_SKinematicConstraint constraint => constraint.DeepClone(),
                    _ => null
                } : null;
                if (model is null)
                {
                    return null;
                }
                ents.Add(new CPlugPrefab.EntRef { Model = model, Position = entity.Position, Rotation = entity.Rotation, Params = entity.Params, U01 = entity.U01 });
            }
            var copy = (CPlugPrefab)prefab.DeepClone();
            copy.Ents = [.. ents];
            return copy;
        }

        private CPlugDynaObjectModel? InvisibleDyna(CPlugDynaObjectModel dyna, Gbx owner, string path, MaterialDressing dressing)
        {
            var ancestor = owner.RefTable?.AncestorLevel ?? 0;
            T? Load<T>(T? inline, GbxRefTableFile? file) where T : CMwNod => inline ?? (file is null ? null : paks.Open(PakStore.Logical(path, ancestor, file.FilePath))?.Node as T);
            if (Load(dyna.Mesh, dyna.MeshFile) is not { } solid)
            {
                return null;
            }
            var copy = (CPlugDynaObjectModel)dyna.DeepClone();
            var dynaShape = Load(dyna.DynaShape, dyna.DynaShapeFile);
            var staticShape = Load(dyna.StaticShape, dyna.StaticShapeFile);
            copy.DynaShape = dynaShape is null ? null : mesh.InlineSurface(dynaShape, owner, path, dressing, CollisionMaterial());
            copy.StaticShape = staticShape is null ? null : mesh.InlineSurface(staticShape, owner, path, dressing, CollisionMaterial());
            var extent = new[] { dynaShape, staticShape }.Select(shape => shape?.Surf).OfType<CPlugSurface.Mesh>().SelectMany(surface => surface.Vertices ?? []).ToList();
            copy.Mesh = mesh.InvisibleSolid(solid, extent);
            copy.MeshFile = null;
            copy.DynaShapeFile = null;
            copy.StaticShapeFile = null;
            return copy;
        }

        private CPlugMaterial CollisionMaterial() => paks.Open(CollisionMaterialTemplate)?.Node as CPlugMaterial ?? throw new InvalidOperationException($"Missing {CollisionMaterialTemplate}");

        private IReadOnlyDictionary<string, string> SkinRemap(string modifierPath)
        {
            var remap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (paks.Open(modifierPath) is not { } modifier || modifier.RefTable?.Files.FirstOrDefault(file => file.FilePath.EndsWith(GameSkinExtension, StringComparison.OrdinalIgnoreCase)) is not { } skinFile)
            {
                return remap;
            }
            if (paks.Open(PakStore.Logical(modifierPath, modifier.RefTable.AncestorLevel, skinFile.FilePath))?.Node is not CPlugGameSkin skin)
            {
                return remap;
            }
            foreach (var fid in skin.HeaderFids ?? [])
            {
                var original = fid.Directory;
                var name = PakPath.Name(original);
                var extension = name[name.IndexOf(ExtensionStart)..];
                if (filesByName.Value.TryGetValue(fid.Name + extension, out var replacement))
                {
                    remap[original] = replacement;
                }
            }
            return remap;
        }

        private static Dictionary<string, string> FileIndex(IPakStore paks)
        {
            var index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in paks.Entries)
            {
                index.TryAdd(entry.Split(PakPath.Separator)[^1], entry);
            }
            return index;
        }

        private byte[] InvisiblePrefabShell(SurfaceBlockFile file, CPlugPrefab prefab)
        {
            using var template = assets.Open(Assets.Template(InvisibleTemplate));
            var item = Gbx.Parse<CGameItemModel>(template);
            item.Node.Ident = new Ident(file.Ident, CollectionIds.Of(file.Collection), file.RecordAuthor);
            item.Node.Name = file.Name;
            item.Node.PageName = file.Block.Archetype.Id;
            item.Node.Description = file.Block.Name;
            item.Node.EntityModel = prefab;
            using var stream = new MemoryStream();
            item.Save(stream);
            return stream.ToArray();
        }

        private (string Entity, MaterialDressing Dressing, IReadOnlyDictionary<string, string> Skin)? ItemEntity(string id, Collections collection, int variant)
        {
            foreach (var environment in collection == Collections.Stadium ? [StadiumFolder] : new[] { collection.ToString(), StadiumFolder })
            {
                if (!items.Value.TryGetValue(PakPath.Join(environment, id), out var path) || paks.Open(path) is not { } gbx)
                {
                    continue;
                }
                var ancestor = gbx.RefTable?.AncestorLevel ?? 0;
                var modifier = (gbx.Node as CGameItemModel)?.MaterialModifierFile?.FilePath;
                var dressing = MaterialDressing.For(paks, materials, collection, path, ancestor, modifier);
                var skin = modifier is not null && !modifier.EndsWith(TerrainModifierExtension, StringComparison.OrdinalIgnoreCase) ? SkinRemap(PakStore.Logical(path, ancestor, modifier)) : new Dictionary<string, string>();
                if (gbx.Node is CGameItemModel { EntityModel: NPlugItem_SVariantList list })
                {
                    return (list.Variants.ElementAtOrDefault(variant) ?? list.Variants.FirstOrDefault())?.EntityModelFile is { } file ? (PakStore.Logical(path, ancestor, file.FilePath), dressing, skin) : null;
                }
                var entities = (gbx.RefTable?.Files ?? []).Where(file => EntityExtensions.Any(extension => file.FilePath.EndsWith(extension, StringComparison.OrdinalIgnoreCase))).ToList();
                return entities.Count == 1 ? (PakStore.Logical(path, ancestor, entities[0].FilePath), dressing, skin) : null;
            }
            return null;
        }

        private bool Animated(string path)
        {
            if (paks.Open(path) is not { Node: CPlugPrefab prefab } gbx)
            {
                return false;
            }
            foreach (var entity in prefab.Ents)
            {
                if (entity.Model is { } model && model is not (CPlugStaticObjectModel or NPlugTrigger_SSpecial))
                {
                    return true;
                }
                if (entity.ModelFile?.FilePath is not { } file || SilentEntityExtensions.Any(extension => file.EndsWith(extension, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }
                if (!EntityExtensions.Any(extension => file.EndsWith(extension, StringComparison.OrdinalIgnoreCase)) || Animated(PakStore.Logical(path, gbx.RefTable!.AncestorLevel, file)))
                {
                    return true;
                }
            }
            return false;
        }

        private static Dictionary<string, string> ItemIndex(IPakStore paks)
        {
            var index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in paks.Entries)
            {
                var segments = entry.Split(PakPath.Separator);
                if (segments.Length > 2 && segments[1] == ItemsFolder && entry.EndsWith(ItemExtension, StringComparison.OrdinalIgnoreCase))
                {
                    index.TryAdd(PakPath.Join(segments[0], segments[^1][..^ItemExtension.Length]), entry);
                }
            }
            return index;
        }

        private CPlugStaticObjectModel Merge(SurfaceBlockFile file, List<PrefabParts.Part> parts, string infoPath, CGameCtnBlockInfo info, int ancestor)
        {
            var family = DeckFamily.Of(file.Block.Archetype) ?? throw new InvalidOperationException($"{file.Block.Archetype.Id} is not a road kind");
            return mesh.Merge(parts, family, SurfaceTarget.Of(file.Block.Surface, family), MaterialDressing.For(paks, materials, file.Collection, infoPath, info, ancestor));
        }

        private byte[] InvisibleShell(SurfaceBlockFile file, CPlugStaticObjectModel model)
        {
            using var template = assets.Open(Assets.Template(InvisibleTemplate));
            var item = Gbx.Parse<CGameItemModel>(template);
            item.Node.Ident = new Ident(file.Ident, CollectionIds.Of(file.Collection), file.RecordAuthor);
            item.Node.Name = file.Name;
            item.Node.PageName = file.Block.Archetype.Id;
            item.Node.Description = file.Block.Name;
            ((CGameCommonItemEntityModel)item.Node.EntityModel!).StaticObject = model;
            using var stream = new MemoryStream();
            item.Save(stream);
            return stream.ToArray();
        }

        private byte[] Shell(SurfaceBlockFile file, CGameCtnBlockInfo info, CGameCtnBlockInfoVariant variant, CPlugStaticObjectModel model, string infoPath, int ancestor)
        {
            using var template = assets.Open(Assets.Template(Template));
            var item = Gbx.Parse<CGameItemModel>(template);
            var collection = CollectionIds.Of(file.Collection);
            item.Node.Ident = new Ident(file.Ident, collection, file.RecordAuthor);
            item.Node.Name = file.Name;
            item.Node.PageName = file.Block.Archetype.Id;
            item.Node.Description = file.Block.Name;
            item.Node.IconWebP = info.IconWebP;
            item.Node.WaypointType = (CGameItemModel.EWaypointType)info.WayPointType;
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
                    if (segments.Length > BlockInfoDepth && BlockInfoFolders.Contains(PakPath.Join(segments[1], segments[2])) && BlockInfoExtensions.Any(extension => entry.EndsWith(extension, StringComparison.OrdinalIgnoreCase)))
                    {
                        byFileName.TryAdd(PakPath.Join(segments[0], segments[^1][..segments[^1].IndexOf(ExtensionStart)]), entry);
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

        private (string Path, CGameCtnBlockInfo Info)? BlockInfo(Block archetype, Collections collection)
        {
            foreach (var environment in collection == Collections.Stadium ? [StadiumFolder] : new[] { collection.ToString(), StadiumFolder })
            {
                var key = PakPath.Join(environment, archetype.Id);
                if (blockInfos.Value.Path(key) is { } path && paks.Open(path)?.Node is CGameCtnBlockInfo info)
                {
                    return (path, info);
                }
            }
            return null;
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
