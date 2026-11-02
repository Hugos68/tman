using System.Collections.Generic;
using System.Text;
using Tman.Core.Surfaces;
using Tman.Core.Surfaces.Building;

namespace Tman.Tests.Fakes
{
    internal sealed class FixedSurfaceBlocks : ISurfaceBlocks
    {
        public List<SurfaceBlockFile> Requested { get; } = [];

        public byte[] Bytes(SurfaceBlockFile file)
        {
            Requested.Add(file);
            return Encoding.UTF8.GetBytes(file.FileName);
        }
    }
}
