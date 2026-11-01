using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Tman.Paks.Installations
{
    internal static class EpicInstalls
    {
        private const string ManifestsFolder = @"Epic\EpicGamesLauncher\Data\Manifests";

        private const string ManifestExtension = "*.item";

        private const string InstallLocationProperty = "InstallLocation";

        public static IEnumerable<string> GameFolders()
        {
            var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var manifests = Path.Combine(programData, ManifestsFolder);
            if (programData.Length == 0 || !Directory.Exists(manifests))
            {
                return [];
            }
            return [.. Directory.EnumerateFiles(manifests, ManifestExtension)
                .Select(manifest => InstallLocation(File.ReadAllText(manifest)))
                .Where(location => location is not null)
                .Select(location => location!)];
        }

        public static string? InstallLocation(string manifest)
        {
            using var document = JsonDocument.Parse(manifest);
            return document.RootElement.TryGetProperty(InstallLocationProperty, out var location) && location.ValueKind == JsonValueKind.String
                ? location.GetString()
                : null;
        }
    }
}
