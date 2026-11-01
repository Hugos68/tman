using System;
using Tman.Paks;

namespace Tman.Core.Game
{
    internal static class GamePaks
    {
        private static readonly Lazy<string> Folder = new(PakDirectory.Find);

        private static readonly Lazy<PakStore> Opened = new(() => new PakStore(Folder.Value));

        public static string Directory => Folder.Value;

        public static PakStore Store => Opened.Value;
    }
}
