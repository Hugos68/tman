using System;
using System.CommandLine;
using System.IO;

namespace tman.Generator
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var paksOption = new Option<DirectoryInfo>("--paks")
            {
                Description = "The directory with Stadium.pak, RedIsland.pak, WhiteShore.pak, GreenCoast.pak and BlueBay.pak.",
                Required = true
            };

            var coreOption = new Option<DirectoryInfo>("--core")
            {
                Description = "The TMAN.CORE project directory (Inventory/Generated is written, Assets/Items is scanned).",
                DefaultValueFactory = _ => new DirectoryInfo(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "TMAN.CORE")))
            };

            var rootCommand = new RootCommand("Regenerate the TMAN.CORE inventory catalog from the game's pak files and the custom items.");
            rootCommand.Options.Add(paksOption);
            rootCommand.Options.Add(coreOption);
            rootCommand.SetAction(parseResult =>
            {
                var core = parseResult.GetValue(coreOption)!.FullName;
                CatalogGenerator.Run(parseResult.GetValue(paksOption)!.FullName, Path.Combine(core, "Inventory", "Generated"), core, Path.Combine("Assets", "Items"));
                return 0;
            });

            return rootCommand.Parse(args).Invoke();
        }
    }
}
