using System.IO;

namespace Tman.Core.Game
{
    public sealed class DirectoryAssets(string root) : IAssets
    {
        public Stream Open(string path) => File.OpenRead(Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar)));
    }
}
