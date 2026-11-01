using System;

namespace Tman.Paks
{
    public static class PakPath
    {
        public const char Separator = '\\';

        public static string Name(string path) => path[(path.LastIndexOf(Separator) + 1)..];

        public static string Folder(string path) => path[..Math.Max(path.LastIndexOf(Separator), 0)];

        public static string Environment(string path) => path[..Math.Max(path.IndexOf(Separator), 0)];

        public static string Join(string folder, string name) => folder.Length == 0 ? name : folder + Separator + name;
    }
}
