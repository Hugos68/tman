using System.IO;

namespace Tman.Core.Game
{
    public interface IAssets
    {
        Stream Open(string path);
    }
}
