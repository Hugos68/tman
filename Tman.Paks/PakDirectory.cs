using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tman.Paks.Installations;

namespace Tman.Paks
{
    public static class PakDirectory
    {
        public const string FolderName = "Paks";

        private const string PacksFolder = "Packs";

        private const string GameFolder = "Trackmania";

        private static readonly string[] LauncherRoots =
        [
            @"Ubisoft\Ubisoft Game Launcher\games",
            @"Steam\steamapps\common",
            @"Epic Games"
        ];

        public static string Find()
        {
            var candidates = Installations().Select(game => Path.Combine(game, PacksFolder)).Concat(Ancestors()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            return candidates.FirstOrDefault(Holds)
                ?? throw new DirectoryNotFoundException($"No Trackmania installation found and no {FolderName}/ folder with the game's {PakStore.Extension} files above {AppContext.BaseDirectory}; looked in: {string.Join(", ", candidates)}");
        }

        private static IEnumerable<string> Installations() =>
            SteamLibraries.GameFolders()
                .Concat(UbisoftInstalls.GameFolders())
                .Concat(EpicInstalls.GameFolders())
                .Concat(ProgramFilesGameFolders());

        private static IEnumerable<string> ProgramFilesGameFolders()
        {
            foreach (var programs in new[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) }.Where(folder => folder.Length > 0).Distinct())
            {
                foreach (var root in LauncherRoots)
                {
                    yield return Path.Combine(programs, root, GameFolder);
                }
            }
        }

        private static IEnumerable<string> Ancestors()
        {
            for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            {
                yield return Path.Combine(directory.FullName, FolderName);
            }
        }

        private static bool Holds(string directory) => PakStore.Paks.All(pak => File.Exists(Path.Combine(directory, pak.Name + PakStore.Extension)));
    }
}
