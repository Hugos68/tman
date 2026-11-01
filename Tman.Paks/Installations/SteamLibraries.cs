using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Tman.Paks.Installations
{
    internal static class SteamLibraries
    {
        private const string SteamKey = @"Software\Valve\Steam";

        private const string SteamPathValue = "SteamPath";

        private const string LibraryFoldersFile = @"steamapps\libraryfolders.vdf";

        private const string GameFolder = @"steamapps\common\Trackmania";

        private static readonly Regex LibraryPath = new("\"path\"\\s+\"([^\"]+)\"", RegexOptions.Compiled);

        public static IEnumerable<string> GameFolders()
        {
            var steam = SteamFolder();
            if (steam is null)
            {
                return [];
            }
            var libraries = new List<string> { steam };
            var libraryFolders = Path.Combine(steam, LibraryFoldersFile);
            if (File.Exists(libraryFolders))
            {
                libraries.AddRange(Parse(File.ReadAllText(libraryFolders)));
            }
            return libraries.Distinct(StringComparer.OrdinalIgnoreCase).Select(library => Path.Combine(library, GameFolder));
        }

        public static IEnumerable<string> Parse(string libraryFolders) =>
            LibraryPath.Matches(libraryFolders).Select(match => match.Groups[1].Value.Replace(@"\\", @"\"));

        private static string? SteamFolder() => OperatingSystem.IsWindows() ? WindowsSteamFolder() : null;

        [SupportedOSPlatform("windows")]
        private static string? WindowsSteamFolder()
        {
            using var key = Registry.CurrentUser.OpenSubKey(SteamKey);
            return key?.GetValue(SteamPathValue) as string is { Length: > 0 } path ? Path.GetFullPath(path) : null;
        }
    }
}
