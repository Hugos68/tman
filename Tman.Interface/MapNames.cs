using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Tman.Interface
{
    public static class MapNames
    {
        private const char FormatMarker = '$';

        private const int ColorCodeLength = 3;

        private const char LinkStyle = 'l';

        private const char ManialinkStyle = 'h';

        private const char PlayerLinkStyle = 'p';

        private const char LinkOpen = '[';

        private const char LinkClose = ']';

        private const char Replacement = '_';

        private static readonly char[] Invalid = [.. Path.GetInvalidFileNameChars().Concat(['/', '\\', ':', '*', '?', '"', '<', '>', '|']).Distinct()];

        public static string FileName(string mapName)
        {
            var plain = Deformat(mapName).Trim();
            var safe = new string([.. plain.Select(c => Invalid.Contains(c) || char.IsControl(c) ? Replacement : c)]).Trim(' ', '.');
            return safe.Length == 0 ? Replacement.ToString() : safe;
        }

        public static string Deformat(string formatted)
        {
            var text = new StringBuilder(formatted.Length);
            for (var i = 0; i < formatted.Length; i++)
            {
                var c = formatted[i];
                if (c != FormatMarker)
                {
                    text.Append(c);
                    continue;
                }
                if (i + 1 >= formatted.Length)
                {
                    break;
                }
                var code = formatted[++i];
                if (code == FormatMarker)
                {
                    text.Append(FormatMarker);
                }
                else if (Uri.IsHexDigit(code))
                {
                    i += Math.Min(ColorCodeLength - 1, formatted.Length - 1 - i);
                }
                else if (char.ToLowerInvariant(code) is LinkStyle or ManialinkStyle or PlayerLinkStyle && i + 1 < formatted.Length && formatted[i + 1] == LinkOpen)
                {
                    var close = formatted.IndexOf(LinkClose, i + 1);
                    i = close < 0 ? formatted.Length : close;
                }
            }
            return text.ToString();
        }
    }
}
