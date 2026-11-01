using System;
using System.IO;

namespace Tman.Paks
{
    public static class PakDirectory
    {
        public const string FolderName = "Paks";

        public static string Find()
        {
            for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            {
                var candidate = Path.Combine(directory.FullName, FolderName);
                if (File.Exists(Path.Combine(candidate, PakStore.Paks[0].Name + ".pak")))
                {
                    return candidate;
                }
            }
            throw new DirectoryNotFoundException($"No {FolderName}/ folder with the game's .pak files found above {AppContext.BaseDirectory}");
        }
    }
}
