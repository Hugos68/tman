using System.IO;
using Tman.Core.Catalog;

namespace Tman.Core.Surfaces
{
    internal sealed record SurfaceBlockFile(SurfaceBlock Block, bool Ground, int AdditionalVariant, Collections Collection)
    {
        private const string Author = "tman (author:tman)";

        private const string RootFolder = "tman";

        private const string GroundFolder = "Ground";

        private const string AirFolder = "Air";

        private const string Extension = ".Block.Gbx";

        private const string CustomBlockSuffix = "_CustomBlock";

        private const string EmbeddedRoot = "Blocks/";

        public string Name => $"{Block.Name}_{SurfaceBlockKey.Of(Ground, AdditionalVariant)}{EnvironmentSuffix}";

        public string FileName => Name + Extension;

        public string Ident => $@"{RootFolder}\{Block.Surface.Name}\{Folder}\{FileName}";

        public string RecordName => Ident + CustomBlockSuffix;

        public string RecordAuthor => Author;

        public string EmbeddedEntry => EmbeddedRoot + Ident.Replace('\\', '/');

        public string CachePath => Path.Combine(Block.Surface.Name, Folder, FileName);

        private string Folder => Ground ? GroundFolder : AirFolder;

        private string EnvironmentSuffix => Collection == Collections.Stadium ? "" : Collection.ToString();
    }
}
