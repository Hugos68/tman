using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tman.Interface
{
    public sealed record MapPaths(IReadOnlyList<string> Inputs, string Output)
    {
        private const string MapExtension = ".Map.Gbx";

        private const char Wildcard = '*';

        private static readonly char[] WildcardCharacters = ['*', '?'];

        public static MapPaths Resolve(string input, string output)
        {
            var inputs = Expand(input);
            var wildcardInput = IsWildcard(Path.GetFileName(input));
            if (wildcardInput && !output.Contains(Wildcard))
            {
                throw new ArgumentException($"When using a wildcard input, the output path must contain '{Wildcard}'.");
            }
            if (!wildcardInput && output.Contains(Wildcard))
            {
                throw new ArgumentException($"The output path cannot contain '{Wildcard}' when using a single input file.");
            }
            return new MapPaths(inputs, output);
        }

        public static MapPaths ForFolder(IReadOnlyList<string> inputs, string outputFolder) =>
            new(inputs, Path.Combine(outputFolder, Wildcard + MapExtension));

        public string OutputFor(string mapName) =>
            Path.GetFullPath(Output.Contains(Wildcard) ? Output.Replace(Wildcard.ToString(), MapNames.FileName(mapName)) : Output);

        private static string[] Expand(string input)
        {
            var fullPath = Path.GetFullPath(input);
            var directory = Path.GetDirectoryName(fullPath)!;
            var pattern = Path.GetFileName(fullPath);
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"Input directory not found: {directory}");
            }
            var files = IsWildcard(pattern) ? Directory.GetFiles(directory, pattern) : File.Exists(fullPath) ? [fullPath] : [];
            if (files.Length == 0)
            {
                throw new FileNotFoundException($"No input files found: {input}");
            }
            return [.. files.Order(StringComparer.OrdinalIgnoreCase)];
        }

        private static bool IsWildcard(string fileName) => fileName.IndexOfAny(WildcardCharacters) >= 0;
    }
}
