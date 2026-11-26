using System.IO;
using Tman.Core.Catalog;

namespace Tman.Core.Surfaces
{
    internal sealed record SurfaceBlockFile(SurfaceBlock Block, bool Ground, int AdditionalVariant, int BaseVariant, Collections Collection, bool Item = false)
    {
        private const string Author = "tman (author:tman)";

        private const string RootFolder = "tman";

        private const string GroundFolder = "Ground";

        private const string AirFolder = "Air";

        private const string ItemFolder = "Items";

        private const string BlockExtension = ".Block.Gbx";

        private const string ItemExtension = ".Item.Gbx";

        private const string CustomBlockSuffix = "_CustomBlock";

        private const string EmbeddedBlocksRoot = "Blocks/";

        private const string EmbeddedItemsRoot = "Items/";

        public string Name => $"{Block.Name}_{SurfaceBlockKey.Of(Ground, AdditionalVariant) + BaseVariant}{EnvironmentSuffix}";

        public bool Invisible => Block.Surface == Surface.Invisible || Block.Surface == Surface.SeeThrough;

        public string FileName => Name + (Invisible ? ItemExtension : BlockExtension);

        public string Ident => $@"{RootFolder}\{Block.Surface.Name}\{Folder}\{FileName}";

        public string RecordName => Ident + CustomBlockSuffix;

        public string RecordAuthor => Author;

        public string EmbeddedEntry => (Invisible ? EmbeddedItemsRoot : EmbeddedBlocksRoot) + Ident.Replace('\\', '/');

        public string CachePath => Path.Combine(Block.Surface.Name, Folder, FileName);

        private string Folder => Item ? ItemFolder : Ground ? GroundFolder : AirFolder;

        private string EnvironmentSuffix => Collection == Collections.Stadium ? "" : Collection.ToString();
    }
}
