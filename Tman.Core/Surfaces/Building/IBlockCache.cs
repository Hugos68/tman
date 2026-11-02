namespace Tman.Core.Surfaces.Building
{
    public interface IBlockCache
    {
        byte[]? Read(string key);

        void Write(string key, byte[] bytes);
    }
}
