using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Tman.Paks
{
    public static class Names
    {
        public static string Hash(string suffix)
        {
            var bytes = Encoding.UTF8.GetBytes(suffix.ToLowerInvariant());
            var hash = new byte[17];
            hash[0] = (byte)bytes.Length;
            MD5.HashData(bytes).CopyTo(hash, 1);
            var text = new StringBuilder(34);
            foreach (var b in hash)
            {
                text.Append("0123456789ABCDEF"[b & 15]);
                text.Append("0123456789ABCDEF"[b >> 4]);
            }
            return text.ToString();
        }

        public static IEnumerable<string> Candidates(string logical)
        {
            yield return logical;
            var parts = logical.Split('\\');
            for (var i = parts.Length - 1; i >= 0; i--)
            {
                var suffix = string.Join('\\', parts.Skip(i));
                yield return i == 0 ? Hash(suffix) : string.Join('\\', parts.Take(i)) + "\\" + Hash(suffix);
            }
        }
    }
}
