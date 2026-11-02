using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace Tman.Generator
{
    internal sealed class Identifiers
    {
        private const char Replacement = '_';

        private readonly HashSet<string> used = new(StringComparer.Ordinal);

        private readonly Dictionary<string, string> byId = new(StringComparer.Ordinal);

        public string For(string id) => For(id, id);

        public string For(string id, string preferredName)
        {
            var name = Sanitised(preferredName);
            var candidate = name;
            for (var n = 2; !used.Add(candidate); n++)
            {
                candidate = $"{name}{Replacement}{n}";
            }
            byId[id] = candidate;
            return candidate;
        }

        public string Of(string id) => byId[id];

        private static string Sanitised(string text)
        {
            var name = new string(text.Select(c => char.IsAsciiLetterOrDigit(c) || c == Replacement ? c : Replacement).ToArray());
            if (name.Length == 0 || char.IsAsciiDigit(name[0]))
            {
                name = Replacement + name;
            }
            return SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None || SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None ? name + Replacement : name;
        }
    }
}
