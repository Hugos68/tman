# Contributing to tman

## Projects

| Project | Role |
| --- | --- |
| `Tman.Core` | The library. Everything an alteration needs; no user-interface code. |
| `Tman.Interface` | The command line (`tman`). A graphical interface would be another project referencing `Tman.Core`. |
| `Tman.Generator` | A development tool: regenerates the game catalog in `Tman.Core/Inventory/Generated` from the game's pak files and the custom items in `Tman.Core/Assets/Items`. Not needed to run tman. |

`Tman.sln` builds all three.

## Setup

- [.NET 10 SDK](https://dotnet.microsoft.com/download).
- `dotnet build Tman.sln`.
- For `Tman.Generator` only: the game's pak files (`Stadium.pak`, `RedIsland.pak`, `WhiteShore.pak`, `GreenCoast.pak`, `BlueBay.pak`) from a Trackmania 2020 installation.

Before pushing:

```bash
dotnet format --verify-no-changes
dotnet build --configuration Release
```

Both run in CI on every push and pull request.

## How Tman.Core is organised

```
Tman.Core/
├─ Alterations/      the alterations, one folder per family (Cars, Checkpoints, Effects, Finishes)
│  ├─ Alteration.cs  the base class
│  └─ Internal/      reusable steps alterations are composed from
├─ Inventory/        what the game has
│  ├─ Block.cs, Item.cs, Collections.cs, Waypoint.cs, Car.cs   the hand-written types
│  ├─ Generated/     Blocks.g.cs, Items.g.cs, Custom.g.cs — written by Tman.Generator, never edited by hand
│  └─ Builders/      items built at run time (FinishPlane: one finish item per map)
├─ Maps/             a map and what is in it: Map, Placement, Selector, positioning
└─ Assets/           Data/ (lookup tables) and Items/<Category>/<Family>/*.Item.Gbx (the custom items)
```

Namespaces follow the folders (`Tman.Core.Alterations.Finishes`, `Tman.Core.Inventory`, `Tman.Core.Maps`).

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

An alteration is a class deriving from `Alteration` in `Tman.Core.Alterations.<Family>`; its class name is the name users type. Most alterations are a list of steps:

```csharp
public class Checkpointless : Alteration
{
    protected override IEnumerable<Alteration> PreappliedAlterations =>
    [
        new ValidationRemoved(),
        new LightmapRemoved(),
        new InventoryRemoved(placement => placement.IsCheckpoint),
        new MapNameEdited(name => name + " Checkpointless")
    ];
}
```

Override `Run(Map map)` when the alteration needs logic of its own. Start every alteration with `ValidationRemoved` and `LightmapRemoved` and end it with `MapNameEdited`; use `ItemFinishes` to place a finish item at every placement matching a selector, `WaypointEffect` subclasses for gates at starts and checkpoints, `ConditionalAlteration` for environment-specific steps. Add the alteration to the table in `README.md`.

Rules of the house: no comments, no optional knobs, no strings where the catalog has a name, nothing that reads a file outside `Assets/`. Fail loudly (throw) rather than skip when a map cannot be altered as asked.

## Custom items

Items live in `Tman.Core/Assets/Items/<Category>/<Family>/`. An item's internal name is its file name, so an item is never renamed — it is regenerated under the new name. Items are built from the game's own meshes, materials and trigger shapes so they look and behave like the originals; per-environment variants (`…RedIsland`, `…WhiteShore`, `…GreenCoast`, `…BlueBay`) carry the terrain game skin the game applies to Stadium pieces on vista maps. After adding or changing items, run the generator so `Custom.g.cs` knows them.

## Regenerating the catalog

```bash
dotnet run --project Tman.Generator -- --paks <directory with the five .pak files>
```

Writes `Blocks.g.cs`, `Items.g.cs` and `Custom.g.cs` into `Tman.Core/Inventory/Generated` (`--core` points at another `Tman.Core` directory). Run it after a game update that changes blocks or items, and after any change in `Assets/Items`. The generated files are committed; a diff in them is reviewed like any other.

## Verifying a change

tman has no automated tests yet. The check that catches most regressions: run the alteration you touched (and the ones sharing its steps) on a few maps of each environment before and after your change and compare the outputs — the block and item lists must be identical unless the change meant to move them. Open the result in the editor or drive it when the change is visible or affects gameplay.
