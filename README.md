# tman (Trackmania Altered Nadeo)

tman is a CLI tool for programmatically applying predefined alterations to Trackmania maps.

It provides a reusable way to modify .Map.Gbx files through composable alterations, allowing map creators to automate repetitive editing tasks and generate modified versions of existing maps.

## Features

- **CLI-based workflow** — process maps directly from the command line.

- **Composable alterations** — combine multiple alterations to produce the desired result.

- **Automated map editing** — apply changes without manually editing each map in the Trackmania editor.

- **Batch-friendly** — integrate map generation into scripts and other automated workflows.

## Usage

Pass a map to tman along with the alterations you want to apply and an output path.

dotnet run -- "Straight to the Finish" \
  -i "input.Map.Gbx" \
  -o "output.Map.Gbx"

The example applies the Straight to the Finish alteration to the input map and writes the result to the specified output file.

## Alterations

Alterations define individual modifications that can be applied to a map. They can be combined to build more complex transformations from smaller, reusable operations.

Examples of potential alteration categories include:

Waypoints — modify start, checkpoint, and finish behavior.

Blocks and items — place, replace, or modify map elements.

Map properties — change metadata and other map settings.

The available alterations and their specific behavior depend on the implementation.

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