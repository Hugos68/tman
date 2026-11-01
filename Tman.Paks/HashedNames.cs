using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Tman.Paks
{
    internal static class HashedNames
    {
        private const string HexDigits = "0123456789ABCDEF";

        private const int LowNibble = 0xF;

        private const int NibbleBits = 4;

        public static string Hash(string suffix)
        {
            var bytes = Encoding.UTF8.GetBytes(suffix.ToLowerInvariant());
            byte[] hash = [(byte)bytes.Length, .. MD5.HashData(bytes)];
            var text = new StringBuilder(hash.Length * 2);
            foreach (var value in hash)
            {
                text.Append(HexDigits[value & LowNibble]);
                text.Append(HexDigits[value >> NibbleBits]);
            }
            return text.ToString();
        }

        public static IEnumerable<string> Candidates(string logical)
        {
            yield return logical;
            var parts = logical.Split(PakPath.Separator);
            for (var i = parts.Length - 1; i >= 0; i--)
            {
                var suffix = string.Join(PakPath.Separator, parts.Skip(i));
                yield return PakPath.Join(string.Join(PakPath.Separator, parts.Take(i)), Hash(suffix));
            }
        }
    }
}
