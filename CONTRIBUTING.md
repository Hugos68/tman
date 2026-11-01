# Contributing to tman

## Projects

| Project | Role |
| --- | --- |
| `Tman.Core` | The library. Everything an alteration needs; no user-interface code. |
| `Tman.Interface` | The command line (`tman`). A graphical interface would be another project referencing `Tman.Core`. |
| `Tman.Paks` | Reads the game's pak files: the per-pak keys, the hashed-name resolver, the `Paks/` folder lookup. Used by `Tman.Core` (surface blocks) and `Tman.Generator`. |
| `Tman.Tests` | Unit tests of the rules in `Tman.Core` (`dotnet test`). |
| `Tman.Generator` | A development tool: regenerates the game catalog in `Tman.Core/Catalog/Generated` from the game's pak files and the custom items in `Tman.Core/Assets/Items`. Not needed to run tman. |

`Tman.sln` builds all five.

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
│  ├─ Alteration.cs          the base class: pre-applied steps, Run, a new map UID
│  ├─ StandardAlteration.cs  what every user-facing alteration derives from: validation and lightmap removed, Steps, Run, the map renamed
│  ├─ Finishes/              the finish alterations and FinishPlane, the finish item built per map
│  ├─ Steps/                 the reusable steps alterations are composed from (each one is itself an Alteration)
│  │  └─ Waypoints/          the steps that act on starts, checkpoints and finishes
│  └─ Surfaces/              SurfaceAlteration (the mechanism) and one three-line class per surface
├─ Catalog/                  what the game has: Block, Item, Collections, Waypoint, Car, BlockFlags, CollectionIds
│  └─ Generated/             Blocks.g.cs, Items.g.cs, Custom.g.cs — written by Tman.Generator, never edited by hand
├─ Surfaces/                 the surface alterations' vocabulary and rules: Surface, SurfaceTarget, SurfaceRules, DeckFamily, SurfaceBlock, SurfaceBlockKey, SurfaceBlockFile
│  └─ Building/              one custom block per road kind, variant, environment and surface, built from the game's paks: SurfaceBlockBuilder, SurfaceBlockMesh, GameMaterials, MaterialDressing, LightmapAtlas, LodLevels, SurfaceBlockCache
├─ Game/                     the game's own files: GamePaks (the pak store), PrefabParts (a prefab flattened into placed static objects), Transforms
├─ Maps/                     a map and what is in it
│  ├─ Map.cs                 load, save, metadata, queries, removal
│  ├─ Map.Placing.cs         Place, PlaceRelative, PlaceLike
│  ├─ Map.Replacing.cs       Replace (by block, item, waypoint kind or surface block)
│  ├─ Map.Moving.cs          Free, Nudge, Translate
│  ├─ Map.Waypoints.cs       waypoint properties and checkpoint links
│  ├─ Map.Embedding.cs       custom items and surface blocks embedded into the map
│  ├─ Map.Cleaning.cs        validation and lightmap removal
│  ├─ Placements/            Placement (a block or item in the map), Selector, PlacementExtensions
│  └─ Geometry/              Position, Direction, DirectionOffset, MapConstants
└─ Assets/                   Data/ (lookup tables, the game-made custom block shell), Items/<Category>/<Family>/*.Item.Gbx (custom items)
```

Namespaces follow the folders (`Tman.Core.Alterations.Finishes`, `Tman.Core.Catalog`, `Tman.Core.Surfaces.Building`, `Tman.Core.Maps.Placements`). `Map` is one partial class split by concern: call sites read `map.Replace(...)`, the file name tells which concern the method belongs to.

Everything is `internal` unless Tman.Interface needs it (`Alteration`, the alterations, `Map.Load` and `Map.Save`), every class that is not a base class is `sealed`, and every number that is not zero or one has a name.

### The catalog

`Blocks`, `Items` and `Custom` are generated classes with one static field per game block, game item and custom item: `Blocks.RoadTechStraight`, `Items.GateCheckpointCenter32m`, `Custom.FinishFlag16m`. Every field is a record carrying what the game knows about it — size in cells, footprint cells, which collections have it, its waypoint kind, its variants — so alterations never pass around strings. Useful members:

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

A surface block is a custom block built on the fly from the game's paks by `SurfaceBlockBuilder`: the environment's block definition (vista maps have their own geometry for Stadium kinds), the record's variant (ground or air, additional index), its prefab's entities merged into one static object — inline static objects, geometry sub-prefabs and the kerbs; the `TreeGen` lamps and supports and the vegetation placements are left out because the game draws those itself around a custom block — with the deck slot's material and physics swapped to the surface (`RoadFamily.DeckMaterial` → `SurfaceTarget`), every other material keeping the surface and gameplay ids the pak gives it. The mesh goes into the item shell the game itself writes for a custom block (`Assets/Data/SurfaceBlock.Block.Gbx`): archetype = the original kind, the map's collection, the variant key `SurfaceBlockKey.Of(ground, additionalVariant)` (the game's own rule: ground 0, air 1 000 000, plus 1 000 per additional variant). Built blocks are cached in `Cache/Blocks/<pak stamp>/` next to `Paks/` (gitignored); a game update changes the stamp and the cache rebuilds itself. `SurfaceBlockFile` names the file and the record (`<Surface><Kind>_<key>[<Environment>].Block.Gbx`, record `tman\<Surface>\<Ground|Air>\<file>_CustomBlock`, `BlockFlags.CustomBlock`), `MapReplacing.Replace(placement, surfaceBlock)` rewrites the record and `MapEmbedding` embeds the bytes deflated (a stored entry crashes the game at load).

## Custom items

Items live in `Tman.Core/Assets/Items/<Category>/<Family>/`. An item's internal name is its file name, so an item is never renamed — it is regenerated under the new name. Items are built from the game's own meshes, materials and trigger shapes so they look and behave like the originals; per-environment variants (`…RedIsland`, `…WhiteShore`, `…GreenCoast`, `…BlueBay`) carry the terrain game skin the game applies to Stadium pieces on vista maps. After adding or changing items, run the generator so `Custom.g.cs` knows them.

## Regenerating the catalog

```bash
dotnet run --project Tman.Generator -- --paks <directory with the five .pak files>
```

Writes `Blocks.g.cs`, `Items.g.cs` and `Custom.g.cs` into `Tman.Core/Catalog/Generated` (`--core` points at another `Tman.Core` directory). Run it after a game update that changes blocks or items, and after any change in `Assets/Items`. The generated files are committed; a diff in them is reviewed like any other.

## Verifying a change

`dotnet test` runs `Tman.Tests`: the pure rules (flag bits, surface block keys and file names, the surface rules, road families, prefab transforms, selectors). The check that catches most regressions in alterations themselves: run the alteration you touched (and the ones sharing its steps) on a few maps of each environment before and after your change and compare the outputs — the block and item lists must be identical unless the change meant to move them. Open the result in the editor or drive it when the change is visible or affects gameplay.
