<img src="assets/banner.png" alt="TMAN - Trackmania Altered Nadeo" width="100%">

# tman (Trackmania Altered Nadeo)

tman applies predefined alterations to Trackmania 2020 maps: give it a `.Map.Gbx` file (or a folder of them) and the name of an alteration, and it writes an altered copy. Map creators use it to produce whole altered campaigns without touching each map in the editor.

## Install

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download). Then:

```bash
git clone https://github.com/Hugos68/tman
cd tman
dotnet build Tman.sln --configuration Release
```

The executable is `Tman.Cli/bin/Release/net10.0/tman` (`tman.exe` on Windows).

There is also a window: `Tman.Gui/bin/Release/net10.0/tman-gui` (`tman-gui.exe`). Pick an alteration on the left, drop map files or a folder of maps onto it (or use the buttons), choose an output folder (by default a folder named after the alteration next to the maps), Run. Progress and the log — the same lines the command line prints, including the warnings — show at the bottom. `dotnet publish Tman.Gui -c Release -r win-x64 --self-contained -p:PublishSingleFile=true` makes a single tman-gui.exe that needs no .NET installed.

## Usage

```bash
tman <alteration> -i <input> -o <output>
```

| Option | Meaning |
| --- | --- |
| `<alteration>` | The alteration's name (case-insensitive), see the table below. |
| `-i`, `--input` | A map file, or a wildcard pattern such as `maps/*.Map.Gbx`. |
| `-o`, `--output` | The output file, or a pattern containing `*` when the input is a pattern (`*` is replaced by each input's name). |
| `-v`, `--verbose` | Log every step, every replaced block, the kinds a step kept and every surface block built. |
| `-q`, `--quiet` | Log warnings and errors only. |

Example — one map:

```bash
tman StraightToTheFinish -i "Fall 2026 - 01.Map.Gbx" -o "Fall 2026 - 01 STTF.Map.Gbx"
```

Example — a campaign:

```bash
tman Flags -i "*.Map.Gbx" -o "* Flags.Map.Gbx"
```

Every output map has its validation removed (no author time, no medals — validate it in the editor), its lightmap removed (the editor recomputes shadows), a new UID and the alteration's name appended to the map name. Alterations that add items embed them in the map; nothing has to be installed on the player's side.

## Alterations

| Alteration | Family | What it does |
| --- | --- | --- |
| `CarDesert` | Cars | Drives the map with the Desert car. |
| `CarRally` | Cars | Drives the map with the Rally car. |
| `CarSnow` | Cars | Drives the map with the Snow car. |
| `CarSport` | Cars | Drives the map with the Sport (Stadium) car. |
| `Checkpointless` | Checkpoints | Removes every checkpoint. |
| `StraightToTheFinish` | Checkpoints | Replaces every checkpoint by its plain block (the road or platform piece it was built from), so the race is start to finish with nothing in between. |
| `StraightToTheCenter` | Finishes | Moves the finish as is to the absolute center of the map |
| `StraightToTheFinishAndBack` | Finishes | Puts a finish inside the start and turns every old finish into a linked checkpoint: drive to the far end and back. Ring finishes become respawnable checkpoint gates; expandable finish gates become expandable checkpoint gates. |
| `Freewheel` | Effects | Turbo and no-engine gates at the start and at every checkpoint. |
| `NoSteer` | Effects | No-steering gates at the start and at every checkpoint. |
| `SlowMotion` | Effects | Slow-motion gates at the start and at every checkpoint. |
| `Flags` | Finishes | Every flag is a finish. |
| `Podium` | Finishes | Every podium is a finish |
| `Screens` | Finishes | Every screen is a finish |
| `FloorFinish` | Finishes | The floor is the finish: the grass on Stadium maps (including the unbuildable grass out to the stands, minus the cells blocks stand on), the water on the vista maps (plus a 16-cell ring around the map). |
| `GroundClippers` | Finishes | A finish slab just under the ground, for cars that clip through it. |
| `Manslaughter` | Finishes | Every spectator stand is a finish. |
| `Moth` | Finishes | Every light is a finish. |
| `Jitter` | Other | Every block/item position and yawpitchroll is slightly changed |
| `Road` | Surfaces | Every road, open road and platform drives on asphalt: road kinds become surface blocks with the asphalt deck, platforms their Tech twin. |
| `Dirt` | Surfaces | The same with dirt. |
| `Ice` | Surfaces | The same with ice: the bobsleigh deck on roads, the ice platform on platforms and open roads. |
| `Grass` | Surfaces | The same with grass. |
| `Plastic` | Surfaces | The same with plastic. |
| `Wood` | Surface | The same with wood. |
| `Metal` | Surface | The same with metal. |
| `Magnet` | Surface | The same with a dark metal deck the car sticks to (the Doomsday super-magnetic physics). |
| `PodiumReverse` | Reverse | The race runs backwards: the car starts in front of the podium (the one nearest the old start when there are several), facing away from it, and the old start is the finish; checkpoints and the old finish become plain blocks. A map without a podium is left as it is. |
 
The surface alterations build their blocks from the game's own files: copy `Stadium.pak`, `RedIsland.pak`, `WhiteShore.pak`, `GreenCoast.pak` and `BlueBay.pak` (from `Trackmania\Packs`). The first run on a map builds every road block it needs (about a second each) into a cache; later runs reuse them. The cache is in the user's local application data (`%LocalAppData%\tman\Cache` on Windows). The cache is keyed by the pak files and the build, so a game update or a code change never serves stale blocks.

A map of a vista environment (RedIsland, WhiteShore, GreenCoast, BlueBay) with embedded custom blocks crashes the game when the editor switches its mood — any embedded block, including blocks the game made itself; the editor was never meant to re-decorate one. Altered vista maps are for playing; set the mood on the source map before altering it. tman logs a warning when it alters such a map.

## Where it comes from

Trackmania and its file format are Nadeo's. tman reads and writes maps with [GBX.NET](https://github.com/BigBang1112/gbx-net). The block and item catalog it ships (`Tman.Core/Inventory/Generated`) is generated from the game's own pak files; the custom items in `Tman.Core/Assets/Items` are built from the game's own meshes and materials.

Developing tman: see [CONTRIBUTING.md](CONTRIBUTING.md).

## License

[MIT](LICENSE.txt).
