using System;
using System.IO;

namespace tman
{
    public static class Assets
    {
        public static readonly string Root = AppContext.BaseDirectory;

        public static string Data(string file) => Path.Combine(Root, "assets", "data", file);
    }
}
