# tman (Trackmania Altered Nadeo)

tman is a CLI tool for programmatically applying predefined alterations to Trackmania maps.

It provides a reusable way to modify .Map.Gbx files through composable alterations, allowing map creators to automate repetitive editing tasks and generate modified versions of existing maps.

## Features

- **CLI-based workflow** — process maps directly from the command line.

- **Composable alterations** — combine multiple alterations to produce the desired result.

- **Automated map editing** — apply changes without manually editing each map in the Trackmania editor.

- **Batch-friendly** — integrate map generation into scripts and other automated workflows.

The example applies the Straight to the Finish alteration to the input map and writes the result to the specified output file.

## Supported Alterations

tman currently supports the following alterations:

| Alteration | Description |
| --- | --- |
| `CarDesert` | Replaces the car with the Desert Car. |
| `CarRally` | Replaces the car with the Rally Car. |
| `CarSnow` | Replaces the car with the Snow Car. |
| `CarSport` | Replaces the car with the Sport Car. |
| `Checkpointless` | Applies the Checkpointless alteration. |
| `Flags` | Applies the Flags alteration. |
| `FloorFinish` | Applies the FloorFinish alteration. |
| `Freewheel` | Applies the Freewheel alteration. |
| `GroundClippers` | Applies the GroundClippers alteration. |
| `Manslaughter` | Applies the Manslaughter alteration. |
| `Moth` | Applies the Moth alteration. |
| `NoSteer` | Applies the NoSteer alteration. |
| `SlowMotion` | Applies the SlowMotion alteration. |
| `StraightToTheFinish` | Applies the StraightToTheFinish alteration. |

Alteration names are case-insensitive. Each alteration can be selected by passing its name as the first argument:

```bash
dotnet run -- "Checkpointless" \
  -i "input.Map.Gbx" \
  -o "output.Map.Gbx"
```

## Development

tman is built with C# and .NET 10, using GBX.NET to work with Trackmania's GBX file format.

### Requirements

.NET 10 SDK

A Trackmania map in .Map.Gbx format

### Build

dotnet restore
dotnet build --configuration Release

#### Formatting

Check that the code follows the expected formatting conventions:

dotnet format --verify-no-changes --no-restore

## License

See the repository's license file for licensing information.