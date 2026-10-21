using System;
using System.IO;

namespace tman.src.Application
{
    public static class ApplicationConstants
    {
        public static string ProjectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
    }
}