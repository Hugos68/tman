using System;
using System.IO;

namespace Tman
{
    public static class Assets
    {
        public static readonly string Root = AppContext.BaseDirectory;

        public static string Data(string file) => Path.Combine(Root, "Assets", "Data", file);
    }
}
