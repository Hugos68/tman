using System.Collections.Generic;
using GBX.NET;

namespace Tman.Paks
{
    public interface IPakStore
    {
        string Directory { get; }

        IEnumerable<string> Entries { get; }

        string? Resolve(string logical);

        Gbx? Open(string logical);

        byte[]? ReadDecompressed(string logical);
    }
}
