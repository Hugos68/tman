using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using GBX.NET;
using GBX.NET.LZO;
using GBX.NET.PAK;

namespace Tman.Paks
{
    public sealed class PakStore : IDisposable
    {
        public const string Extension = ".pak";

        private const int ReadBufferSize = 65536;

        public static readonly IReadOnlyList<(string Name, string Key)> Paks =
        [
            ("Stadium", "B773D73047A4104857722366D78D28A6"),
            ("RedIsland", "660C4C156B80337E296A1034B0AA05B8"),
            ("WhiteShore", "660C4C156B80337E296A1034B0AA05B8"),
            ("GreenCoast", "660C4C156B80337E296A1034B0AA05B8"),
            ("BlueBay", "660C4C156B80337E296A1034B0AA05B8"),
        ];

        private readonly List<Pak> paks = [];
        private readonly Dictionary<string, (Pak Pak, PakFile File)> index = new(StringComparer.Ordinal);
        private readonly List<(string Logical, PakFile File, Pak Pak)> entries = [];
        private readonly Dictionary<string, Gbx?> cache = new(StringComparer.Ordinal);

        public PakStore(string directory)
        {
            Gbx.LZO = new Lzo();
            foreach (var (name, key) in Paks)
            {
                var pak = Pak.Parse(File.OpenRead(Path.Combine(directory, name + Extension)), Convert.FromHexString(key), KeyType.EncryptionKey);
                paks.Add(pak);
                foreach (var (path, file) in pak.Files)
                {
                    var logical = path.Replace('/', PakPath.Separator);
                    var upper = logical.ToUpperInvariant();
                    if (index.TryAdd(upper, (pak, file)))
                    {
                        entries.Add((logical, file, pak));
                    }
                }
            }
        }

        public IEnumerable<string> Entries => entries.Select(e => e.Logical);

        public string? Resolve(string logical)
        {
            foreach (var candidate in HashedNames.Candidates(logical))
            {
                if (index.ContainsKey(candidate.ToUpperInvariant()))
                {
                    return candidate;
                }
            }
            return null;
        }

        public Gbx? Open(string logical)
        {
            if (cache.TryGetValue(logical, out var cached))
            {
                return cached;
            }
            Gbx? gbx = null;
            var stored = Resolve(logical);
            if (stored is not null)
            {
                var (pak, file) = index[stored.ToUpperInvariant()];
                try
                {
                    gbx = pak.OpenGbxFile(file);
                }
                catch (Exception)
                {
                    try
                    {
                        gbx = pak.OpenGbxFileHeader(file);
                    }
                    catch (Exception)
                    {
                        gbx = null;
                    }
                }
            }
            cache[logical] = gbx;
            return gbx;
        }

        public byte[]? ReadDecompressed(string logical)
        {
            var stored = Resolve(logical);
            if (stored is null)
            {
                return null;
            }
            var (pak, file) = index[stored.ToUpperInvariant()];
            try
            {
                using var stream = pak.OpenFile(file, out _);
                var raw = new MemoryStream();
                var buffer = new byte[ReadBufferSize];
                while (true)
                {
                    var n = stream.Read(buffer, 0, buffer.Length);
                    if (n <= 0)
                    {
                        break;
                    }
                    raw.Write(buffer, 0, n);
                }
                raw.Position = 0;
                var output = new MemoryStream();
                Gbx.Decompress(raw, output);
                return output.ToArray();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string Logical(string ownerLogical, int ancestorLevel, string filePath)
        {
            var segments = ownerLogical.Split(PakPath.Separator).ToList();
            segments.RemoveAt(segments.Count - 1);
            for (var i = 0; i < ancestorLevel; i++)
            {
                segments.RemoveAt(segments.Count - 1);
            }
            foreach (var segment in filePath.Replace('/', PakPath.Separator).Split(PakPath.Separator))
            {
                if (segment == "..")
                {
                    segments.RemoveAt(segments.Count - 1);
                }
                else
                {
                    segments.Add(segment);
                }
            }
            return string.Join(PakPath.Separator, segments);
        }

        public void Dispose()
        {
            foreach (var pak in paks)
            {
                pak.Dispose();
            }
        }
    }
}
