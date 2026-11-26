namespace Tman.Core.Surfaces.Building
{
    internal interface ISurfaceBlocks
    {
        byte[]? Bytes(SurfaceBlockFile file);

        byte[]? CustomItemBytes(SurfaceBlockFile file, byte[] item);
    }
}
