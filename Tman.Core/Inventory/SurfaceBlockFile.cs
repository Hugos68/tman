namespace Tman.Core.Inventory
{
    public sealed record SurfaceBlockFile(SurfaceBlock Block, bool Ground, int AdditionalVariant, Collections Collection)
    {
        private const string Author = "tman (author:tman)";

        private const string CustomBlockSuffix = "_CustomBlock";

        public string FileName => $"{Block.Name}_{SurfaceBlockKey.Of(Ground, AdditionalVariant)}{EnvironmentSuffix}.Block.Gbx";

        public string Ident => $@"tman\{Block.Surface.Name}\{Folder}\{FileName}";

        public string RecordName => Ident + CustomBlockSuffix;

        public string RecordAuthor => Author;

        public string EmbeddedEntry => "Blocks/" + Ident.Replace('\\', '/');

        public string CachePath => System.IO.Path.Combine(Block.Surface.Name, Folder, FileName);

        private string Folder => Ground ? "Ground" : "Air";

        private string EnvironmentSuffix => Collection == Collections.Stadium ? "" : Collection.ToString();
    }
}
