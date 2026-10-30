# tman (Trackmania Altered Nadeo)

tman applies predefined alterations to Trackmania 2020 maps: give it a `.Map.Gbx` file (or a folder of them) and the name of an alteration, and it writes an altered copy. Map creators use it to produce whole altered campaigns without touching each map in the editor.

## Install

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download). Then:

```bash
git clone https://github.com/Hugos68/tman
cd tman
dotnet build Tman.sln --configuration Release
```

The executable is `Tman.Interface/bin/Release/net10.0/tman` (`tman.exe` on Windows).

## Usage

```bash
tman <alteration> -i <input> -o <output>
```

| Option | Meaning |
| --- | --- |
| `<alteration>` | The alteration's name (case-insensitive), see the table below. |
| `-i`, `--input` | A map file, or a wildcard pattern such as `maps/*.Map.Gbx`. |
| `-o`, `--output` | The output file, or a pattern containing `*` when the input is a pattern (`*` is replaced by each input's name). |

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
| `FloorFinish` | Finishes | The floor is the finish: the grass on Stadium maps (including the unbuildable grass out to the stands, minus the cells blocks stand on), the water on the vista maps (plus a 16-cell ring around the map). |
| `GroundClippers` | Finishes | A finish slab just under the ground, for cars that clip through it. |
| `Manslaughter` | Finishes | Every spectator stand is a finish. |
| `Moth` | Finishes | Every light is a finish. |
| `Jitter` | Other | Every block/item position and yawpitchroll is slightly changed |

## Where it comes from

Trackmania and its file format are Nadeo's. tman reads and writes maps with [GBX.NET](https://github.com/BigBang1112/gbx-net). The block and item catalog it ships (`Tman.Core/Inventory/Generated`) is generated from the game's own pak files; the custom items in `Tman.Core/Assets/Items` are built from the game's own meshes and materials.

Developing tman: see [CONTRIBUTING.md](CONTRIBUTING.md).

## License

[MIT](LICENSE.txt).
