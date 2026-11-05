# Contributing to tman

## Projects

| Project | Role |
| --- | --- |
| `Tman.Core` | The library. Everything an alteration needs; no user-interface code. |
| `Tman.Interface` | How a host talks to Core: `TmanHost` composes the services (paks, assets, cache, logging) and hands out the pipeline, `MapPaths` resolves input patterns to output paths, `Batch` runs an alteration over maps in parallel with progress. No console, no window. |
| `Tman.Cli` | The command line (`tman`): arguments and console logging over `Tman.Interface`. |
| `Tman.Gui` | The window (`tman-gui`, Avalonia): the alteration list from `AlterationPipeline.Catalog`, drag and drop, pickers, a progress bar fed by `Batch`'s callback and a log pane that is an `ILoggerProvider`. Nothing in it knows Core beyond `Tman.Interface`. |
| `Tman.Paks` | Reads the game's pak files: the per-pak keys, the hashed-name resolver, the `Paks/` folder lookup. Used by `Tman.Core` (surface blocks) and `Tman.Generator`. |
| `Tman.Tests` | xunit tests of the rules and of the pipeline on fakes (`dotnet test`). |
| `Tman.Generator` | A development tool: regenerates the game catalog in `Tman.Core/Catalog/Generated` from the game's pak files and the custom items in `Tman.Core/Assets/Items`. Not needed to run tman. |

`Tman.sln` builds all seven.

## Setup

- [.NET 10 SDK](https://dotnet.microsoft.com/download).
- `dotnet build Tman.sln`.
- The game's pak files (`Stadium.pak`, `RedIsland.pak`, `WhiteShore.pak`, `GreenCoast.pak`, `BlueBay.pak`, from `Trackmania\Packs` of a Trackmania 2020 installation) copied into a `Paks/` folder at the repository root (gitignored). The surface alterations and `Tman.Generator` read them; the other alterations do not need them. The folder is found by walking up from the executable, so `dotnet run` and a published build both see `<repo>/Paks`.

Before pushing:

```bash
dotnet format --verify-no-changes
dotnet build --configuration Release
dotnet test
```

Both run in CI on every push and pull request.

## How Tman.Core is organised

```
Tman.Core/
├─ Alterations/              the alterations, one folder per family (Cars, Checkpoints, Effects, Finishes, Other, Surfaces)
│  ├─ Alteration.cs          the base class: pre-applied steps, Run(map, context), a new map UID
│  ├─ AlterationContext.cs   the services a running alteration may use: assets and surface blocks
│  ├─ AlterationPipeline.cs  the public entry point: resolves an alteration by name from the service provider and applies it
│  ├─ StandardAlteration.cs  what every user-facing alteration derives from: validation and lightmap removed, Steps, Run, the map renamed
│  ├─ Reverses/              PodiumReverse: start at the podium, finish at the old start
│  ├─ Finishes/              the finish alterations; FinishPlane places a modeler item over boxes, WaterFootprints is the table of which subcells of each shore tile variant are deep water
│  ├─ Steps/                 the reusable steps alterations are composed from (each one is itself an Alteration)
│  │  └─ Waypoints/          the steps that act on starts, checkpoints and finishes
│  └─ Surfaces/              SurfaceAlteration (the mechanism) and one three-line class per surface
├─ Catalog/                  what the game has: Block, Item, Collections, Waypoint, Car, BlockFlags, CollectionIds
│  └─ Generated/             Blocks.g.cs, Items.g.cs, Custom.g.cs — written by Tman.Generator, never edited by hand
├─ Surfaces/                 the surface alterations' vocabulary and rules: Surface, SurfaceTarget, SurfaceRules, DeckFamily, SurfaceBlock, SurfaceBlockKey, SurfaceBlockFile
│  └─ Building/              one custom block per road kind, variant, environment and surface, built from the game's paks: SurfaceBlockBuilder, SurfaceBlockMesh, GameMaterials, MaterialDressing, LightmapAtlas, LodLevels; ISurfaceBlocks = CachedSurfaceBlocks over an IBlockCache (FileBlockCache, MemoryBlockCache)
├─ Game/                     the game's own files: IAssets + DirectoryAssets (the shipped Assets/ folder), PrefabParts (a prefab flattened into placed static objects), Transforms, ModelerItem (a mesh-modeler item written from a template and a list of bodies)
├─ Maps/                     a map and what is in it
│  ├─ Map.cs                 load, save, metadata, queries, removal
│  ├─ Map.Placing.cs         Place, PlaceRelative, PlaceLike
│  ├─ Map.Replacing.cs       Replace (by block, item, waypoint kind or surface block)
│  ├─ Map.Moving.cs          Free, Nudge, Translate
│  ├─ Map.Waypoints.cs       waypoint properties and checkpoint links
│  ├─ Map.Embedding.cs       custom items and surface blocks embedded into the map
│  ├─ Map.Cleaning.cs        validation and lightmap removal
│  ├─ Placements/            Placement (a block or item in the map), Selector, PlacementExtensions
│  └─ Geometry/              Position, Direction, DirectionOffset, MapConstants, Box + Boxes (cells → boxes, merging)
└─ Assets/                   Templates/ (the game-made custom block shell, the finish plane item), Items/<Category>/<Family>/*.Item.Gbx (custom items)
```

Namespaces follow the folders (`Tman.Core.Alterations.Finishes`, `Tman.Core.Catalog`, `Tman.Core.Surfaces.Building`, `Tman.Core.Maps.Placements`). `Map` is one partial class split by concern: call sites read `map.Replace(...)`, the file name tells which concern the method belongs to. `Map` owns the game's map object: the `CGameCtnChallenge` is private, and only `Maps/` and `Surfaces/Building/` work with GBX.NET's node classes — an alteration sees placements, idents and bytes, never the game's object graph.

Everything is `internal` unless a host needs it (`AlterationPipeline`, `AddTman`, the service interfaces and their adapters, `Map.Load` and `Map.Save`), every class that is not a base class is `sealed`, and every number that is not zero or one has a name.

### Services

Core touches the outside world through three interfaces, chosen and constructed by the host (`TmanHost` in Tman.Interface) and registered in a `ServiceCollection`: `IPakStore` (the game's pak files; `PakStore` over a directory), `IAssets` (the shipped `Assets/` folder; `DirectoryAssets`), `IBlockCache` (built surface blocks; `FileBlockCache` under the user's local application data, `MemoryBlockCache` for runs that keep nothing). `AddTman()` registers Core's own services on top — the block builder chain, `ISurfaceBlocks`, the `AlterationContext`, every alteration by type — and `AlterationPipeline.Apply(name, map)` resolves the alteration and runs it. Alterations and steps never reach for a static: what they need comes in through the `AlterationContext` parameter of `Run` (`context.Embed(map, item)`, `context.Replace(map, placement, surfaceBlock)`, `context.Open(assetPath)`). A test composes the same container with a `MemoryBlockCache`, a fake `IPakStore` and an in-memory `IAssets`.

### Logging

Core logs through `Microsoft.Extensions.Logging`: the block builder and the cache take an `ILogger<T>` in their constructors; an alteration and its steps log through `context.Logger`, a logger named after the alteration that runs (`Tman.Core.Alterations.Surfaces.Wood`) inside a scope carrying the map's name, so a parallel campaign run stays readable. Information is one line per step with a count (`SurfaceBlocksPlaced: 43 of 833 blocks replaced`, `FinishPlane over 74 floor boxes`, `Renamed to …`) and one per map with the time; Debug names every step, every replaced placement, the kinds a step kept and how many, and every surface block built or served from the cache; Warning is for what the map has and tman does not know (a block outside the catalog, no floor to finish on). Nothing in Core or Tman.Interface writes to the console: Tman.Cli adds the simple console logger, `--verbose` lowers the level to Debug and `--quiet` raises it to Warning.

### The catalog

`Blocks`, `Items` and `Custom` are generated classes with one static field per game block, game item and custom item: `Blocks.RoadTechStraight`, `Items.GateCheckpointCenter32m`, `Custom.FinishFlag16m`. Every field is a record carrying what the game knows about it — size in cells, footprint cells, which collections have it, its waypoint kind, its variants — so alterations never pass around strings. Useful members:

The game's naming conventions are tables in the same generated classes, computed once by the generator and looked up at run time — never string surgery on an id in Core: `Blocks.PlainOfPenalty` (a penalty kind → its plain straight), `Blocks.PlatformFamilyOf` and `Blocks.PlatformTwins` (a platform's family, and the same shape in another family), `Blocks.Families` / `Items.Families` and `Blocks.PlainOf` (waypoint twins), `Items.SpecialGates` (a waypoint item + an effect gate block → the special item), `Custom.Skinned` (a custom item → its environment twin). The rules live in `Tman.Generator/NamingRules.cs`; a game update that renames things fails there, at generation time, not in a map.

- `block.As(Waypoint.Checkpoint)` — the same family's block of another waypoint kind (the checkpoint twin of a finish), `null` when the game has none.
- `block.Plain` — the plain block a checkpoint was built from.
- `block.Variant(n)` / `item.Variant(n)` — a specific variant, for use as a selector.
- `Blocks.FromId(id)`, `Items.FromId(id)`, `Custom.FromId(id)` — lookups by the game's identifier.
- `item.For(map.Collection)` — the per-environment variant of a custom item when one exists (`CheckpointGateRespawnableBlueBay` for a BlueBay map).

### Maps

`Map` wraps the GBX.NET map. Alterations only see `Map` and `Placement`:

- `map.Placements`, `map.Terrain`, `map.Of(selector)`, `map.Waypoints(kind)` — what is in the map. A `Selector` is a block, an item, or a variant of either; `Placement` exposes the placed object's model, cell, direction, variant, ground flag and pose.
- `map.PlaceRelative(placement, model)` — a new block or item at a placement's pose.
- `map.PlaceLike(placement, model)` / `map.PlaceLike(placement, kind)` — a copy of a placement with another model, same cell, direction, variant and colour.
- `map.Replace(placement, model, rotation)` / `map.Replace(placement, kind)` — the same with removal.
- `map.Remove(placement)`, `map.Embed(customItem)`, `map.Link(checkpoints)`, `map.RemoveValidation()`, `map.RemoveLightmap()`, `map.Car = …`.

Facts the code relies on, learnt from the game's own files: a start block connects through its north face and a finish block through its south face, so a finish placed into a start cell is turned by 180°; the one-pole `GateFinish` items are the finish twins of the Left gates when turned by 180°; block flags bits 21–27 are the block's additional-variant index and must travel with a same-family replacement (the game re-places a block whose variant does not fit, and the re-placed block loses its checkpoint links); a checkpoint link is the waypoint property's tag `LinkedCheckpoint` plus a group number shared by the linked members.

## Writing an alteration

An alteration is a class deriving from `StandardAlteration` in `Tman.Core.Alterations.<Family>`; its class name is the name users type. It names itself and lists its steps; the base class removes the validation and the lightmap first and appends the name to the map's name last:

```csharp
public class Checkpointless : StandardAlteration
{
    protected override string DisplayName => "Checkpointless";

    protected override IEnumerable<Alteration> Steps =>
    [
        new PlacementsRemoved(placement => placement.IsCheckpoint)
    ];
}
```

Override `Run(Map map)` when the alteration needs logic of its own (it runs after the steps). Steps live in `Alterations/Steps`: `PlacementsRemoved`, `CheckpointsCleaned`, `BlocksReplaced` (a rule from block to block), `SurfaceBlocksPlaced` (a rule from block to surface block), `RelativelyPlacedItems` (an item at every placement matching a selector), the `WaypointEffect` subclasses for gates at starts and checkpoints, `ConditionalAlteration` for environment-specific steps. A new family is a new folder; the command line discovers alterations by type. Add the alteration to the table in `README.md`.

Rules of the house: no comments, no optional knobs, no strings where the catalog has a name, no unexplained numbers (name the constant where the rule lives), nothing that reads a file outside `Assets/`. Fail loudly (throw) rather than skip when a map cannot be altered as asked.

### Surface alterations

`Road`, `Dirt`, `Ice`, `Grass` and `Plastic` derive from `SurfaceAlteration` and only name their `Surface`. The mechanism, in `SurfaceRules`: penalty kinds become their plain straight; `Platform<Family>…` blocks become the same shape of the target family when the catalog has it; every road and open-road kind (`RoadFamily`: RoadTech, RoadDirt, RoadIce, RoadBump, OpenTech, OpenDirt, OpenGrass, OpenIce) becomes a surface block unless it already is that surface.

A block definition places each prefab with an optional transform (translation + Euler rotation in degrees, X then Y then Z — the 2x1 slope bases are a dead-end prefab turned half a turn, the wall checkpoints a checkpoint prefab tipped onto the wall); `PrefabParts` applies it. A surface block is a custom block built on the fly from the game's paks by `SurfaceBlockBuilder`: the environment's block definition (vista maps have their own geometry for Stadium kinds), the record's variant (ground or air, additional index), its prefab's entities merged into one static object — inline static objects, geometry sub-prefabs and the kerbs; the `TreeGen` lamps and supports and the vegetation placements are left out because the game draws those itself around a custom block — with the deck slot's material and physics swapped to the surface (`RoadFamily.DeckMaterial` → `SurfaceTarget`), every other material keeping the surface and gameplay ids the pak gives it. The mesh goes into the item shell the game itself writes for a custom block (`Assets/Templates/SurfaceBlock.Block.Gbx`): archetype = the original kind, the map's collection, the variant key `SurfaceBlockKey.Of(ground, additionalVariant)` (the game's own rule: ground 0, air 1 000 000, plus 1 000 per additional variant). Built blocks are cached through `IBlockCache` (the host's `FileBlockCache` keeps them under the user's local application data, by a stamp of the pak files and the build); a game update or a new build changes the stamp and the cache rebuilds itself. `SurfaceBlockFile` names the file and the record (`<Surface><Kind>_<key>[<Environment>].Block.Gbx`, record `tman\<Surface>\<Ground|Air>\<file>_CustomBlock`, `BlockFlags.CustomBlock`), `MapReplacing.Replace(placement, surfaceBlock)` rewrites the record and `MapEmbedding` embeds the bytes deflated (a stored entry crashes the game at load).

Each surface names two looks in `SurfaceTarget`: one for road-like decks and one for platform-like ones (a deck family is platform-like when its deck material is PlatformTech — the platforms and the open roads). Ice is the surface that uses both, the bobsleigh `RoadIce` on roads and the `PlatformIce` modifier elsewhere; the others paint the same material on both, and splitting any of them is one line in that table.

## Custom items

Items live in `Tman.Core/Assets/Items/<Category>/<Family>/`. An item's internal name is its file name, so an item is never renamed — it is regenerated under the new name. Items are built from the game's own meshes, materials and trigger shapes so they look and behave like the originals; per-environment variants (`…RedIsland`, `…WhiteShore`, `…GreenCoast`, `…BlueBay`) carry the terrain game skin the game applies to Stadium pieces on vista maps. After adding or changing items, run the generator so `Custom.cs` knows them. Templates (`Assets/Templates/`) are files tman writes into — the custom block shell, the finish plane item — not items; the generator does not list them.

## Regenerating the catalog

```bash
dotnet run --project Tman.Generator -- --paks <directory with the five .pak files>
```

Writes `Blocks.cs`, `Items.cs` and `Custom.cs` into `Tman.Core/Catalog/Generated` (`--core` points at another `Tman.Core` directory). Run it after a game update that changes blocks or items, and after any change in `Assets/Items`. The generated files are committed; a diff in them is reviewed like any other.

## Verifying a change

`dotnet test` runs `Tman.Tests` (no game files needed): the pure rules — surface rules, deck families, block keys and file names, flags, transforms, LOD merging, material gameplay, the catalog's idents and twins, the caches, the installation parsers, the generator's identifiers — and the pipeline composed on fakes: every alteration resolves from the container, an effect alteration runs without the paks, a surface alteration embeds one block per kind and renames every road placement, on `Maps/Test.Map.Gbx` with a fixed `ISurfaceBlocks` in place of the builder. A test that needs a service of its own registers it after `AddTman()`; the last registration wins. The check that catches most regressions in alterations themselves: run the alteration you touched (and the ones sharing its steps) on a few maps of each environment before and after your change and compare the outputs — for a finish, the geometry inside the generated item too (positions and faces of its crystal layers), since the item's name carries the map's new UID and its bytes differ on every run — the block and item lists must be identical unless the change meant to move them. Open the result in the editor or drive it when the change is visible or affects gameplay.
