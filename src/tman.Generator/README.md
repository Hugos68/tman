# tman.Generator

Regenerates `src/tman.Core/Core/Generated/{Blocks,Items,Custom}.g.cs` from the Trackmania 2020 paks and the project's custom items:

    dotnet run --project src/tman.Generator -- --paks <dir with Stadium.pak RedIsland.pak WhiteShore.pak GreenCoast.pak BlueBay.pak> --out src/tman.Core/Core/Generated --project src/tman.Core --custom assets/items
