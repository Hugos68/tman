using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace Tman.Paks.Installations
{
    public static class UbisoftInstalls
    {
        private const string InstallsKey = @"SOFTWARE\WOW6432Node\Ubisoft\Launcher\Installs";

        private const string InstallDirValue = "InstallDir";

        public static IEnumerable<string> GameFolders() => OperatingSystem.IsWindows() ? WindowsGameFolders() : [];

        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        private static List<string> WindowsGameFolders()
        {
            using var installs = Registry.LocalMachine.OpenSubKey(InstallsKey);
            if (installs is null)
            {
                return [];
            }
            return [.. installs.GetSubKeyNames()
                .Select(name => InstallDir(installs, name))
                .Where(directory => directory is { Length: > 0 })
                .Select(directory => Path.GetFullPath(directory!))];
        }

        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        private static string? InstallDir(RegistryKey installs, string name)
        {
            using var install = installs.OpenSubKey(name);
            return install?.GetValue(InstallDirValue) as string;
        }
    }
}
