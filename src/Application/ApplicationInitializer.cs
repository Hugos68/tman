using GBX.NET;
using GBX.NET.LZO;

namespace tman.src.Application
{
    public static class ApplicationInitializer
    {
        public static void Initialize()
        {
            Gbx.LZO = new Lzo();
        }
    }
}