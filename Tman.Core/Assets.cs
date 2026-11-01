using System;
using System.IO;

namespace Tman.Core
{
    internal static class Assets
    {
        public static readonly string Root = AppContext.BaseDirectory;

        public static string Data(string file) => Path.Combine(Root, "Assets", "Data", file);
    }
}
