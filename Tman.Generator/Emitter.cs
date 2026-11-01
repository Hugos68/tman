using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;
using Tman.Paks;

namespace Tman.Generator
{
    internal static class Emitter
    {
        private static readonly string[] Keywords = ["abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while", "record", "var"];

        public static string Identifier(string id)
        {
            var s = new string(id.Select(c => char.IsAsciiLetterOrDigit(c) || c == '_' ? c : '_').ToArray());
            if (s.Length == 0 || char.IsAsciiDigit(s[0]))
            {
                s = "_" + s;
            }
            if (Keywords.Contains(s))
            {
                s += "_";
            }
            return s;
        }

        public static string F3(float v)
        {
            var t = (long)Math.Round((double)v * 1000.0, MidpointRounding.AwayFromZero);
            return (t < 0 ? "-" : "") + (Math.Abs(t) / 1000).ToString(CultureInfo.InvariantCulture) + "." + (Math.Abs(t) % 1000).ToString("D3", CultureInfo.InvariantCulture);
        }

        public static string Waypoint(int? type) => type switch
        {
            0 => "Waypoint.Start",
            1 => "Waypoint.Finish",
            2 => "Waypoint.Checkpoint",
            4 => "Waypoint.StartFinish",
            _ => "Waypoint.None",
        };

        public static string Collections(int mask)
        {
            if (mask == 31)
            {
                return "Collections.All";
            }
            var names = PakStore.Paks.Select((p, i) => (p.Name, Bit: 1 << i)).Where(p => (mask & p.Bit) != 0).Select(p => "Collections." + p.Name).ToList();
            return names.Count == 0 ? "0" : string.Join(" | ", names);
        }

        private static string Unique(string id, HashSet<string> used)
        {
            var s = Identifier(id);
            var baseName = s;
            for (var n = 2; !used.Add(s); n++)
            {
                s = $"{baseName}_{n}";
            }
            return s;
        }

        private static string Kind(int kind) => Waypoint(kind);

        private static void Families(StringBuilder cs, string type, List<(string Name, int Kind, string Twin, bool HalfTurned)> families, Dictionary<string, string> identOf)
        {
            cs.Append($"\n    public static readonly IReadOnlyDictionary<(string, Waypoint), {type}> Families = new Dictionary<(string, Waypoint), {type}>\n    {{\n");
            foreach (var (name, kind, twin, _) in families)
            {
                cs.Append($"        [(\"{name}\", {Kind(kind)})] = {identOf[twin]},\n");
            }
            cs.Append("    };\n");
            if (type == "Item")
            {
                cs.Append("\n    public static readonly IReadOnlySet<(string, Waypoint)> HalfTurned = new HashSet<(string, Waypoint)>\n    {\n");
                foreach (var (name, kind, _, halfTurned) in families.Where(f => f.HalfTurned))
                {
                    cs.Append($"        (\"{name}\", {Kind(kind)}),\n");
                }
                cs.Append("    };\n");
            }
        }

        public static string Blocks(SortedDictionary<string, BlockRow> blocks, List<(string Name, int Kind, string Twin, bool HalfTurned)> families)
        {
            var cs = new StringBuilder();
            cs.Append(CatalogGenerator.Header).Append(CatalogGenerator.Common);
            cs.Append("internal static class Blocks\n{\n");
            var used = new HashSet<string>(StringComparer.Ordinal);
            var idents = new List<(string Ident, string Id)>();
            foreach (var (id, b) in blocks)
            {
                var ident = Unique(id, used);
                var cells = b.Cells is null ? "null" : "new[] { " + string.Join(", ", b.Cells.Select(c => $"new[] {{ {c.X}, {c.Z} }}")) + " }";
                cs.Append($"    public static readonly Block {ident} = new(\"{id}\", {b.SizeX}, {b.SizeZ}, {cells}, {Collections(b.Collections)}, {Waypoint(b.Waypoint)}, {(b.Ground ? "true" : "false")}, {(b.Air ? "true" : "false")}, {b.Variants});\n");
                idents.Add((ident, id));
            }
            cs.Append("\n    public static readonly IReadOnlyDictionary<string, Block> ById = new Dictionary<string, Block>\n    {\n");
            foreach (var (ident, id) in idents)
            {
                cs.Append($"        [\"{id}\"] = {ident},\n");
            }
            cs.Append("    };\n\n    public static Block? FromId(string id) => ById.TryGetValue(id, out var block) ? block : null;\n\n");
            cs.Append("    public static readonly IReadOnlyDictionary<string, Block> PlainOf = new Dictionary<string, Block>\n    {\n");
            var identOf = idents.ToDictionary(p => p.Id, p => p.Ident, StringComparer.Ordinal);
            var twins = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var (checkpoint, plain) in CheckpointTwins.Twins)
            {
                twins[checkpoint] = plain;
            }
            foreach (var (checkpoint, plain) in twins)
            {
                if (identOf.ContainsKey(checkpoint) && identOf.TryGetValue(plain, out var plainIdent))
                {
                    cs.Append($"        [\"{checkpoint}\"] = {plainIdent},\n");
                }
            }
            cs.Append("    };\n");
            Families(cs, "Block", families, identOf);
            cs.Append("}\n");
            return cs.ToString();
        }

        public static string Items(string className, IEnumerable<ItemRow> items, List<(string Name, int Kind, string Twin, bool HalfTurned)> families)
        {
            var cs = new StringBuilder();
            cs.Append(CatalogGenerator.Header).Append(CatalogGenerator.Common);
            cs.Append($"internal static class {className}\n{{\n");
            var used = new HashSet<string>(StringComparer.Ordinal);
            var idents = new List<(string Ident, string Id)>();
            foreach (var it in items)
            {
                var ident = Unique(className == "Items" ? it.Id : it.Id[..^9], used);
                var path = it.Path is null ? "null" : $"\"{it.Path}\"";
                cs.Append($"    public static readonly Item {ident} = new(\"{it.Id}\", \"{it.Author}\", {path}, {Collections(it.Collections)}, {Waypoint(it.Waypoint)}, {it.Variants}, new Vec3({F3(it.Size.X)}f, {F3(it.Size.Y)}f, {F3(it.Size.Z)}f));\n");
                idents.Add((ident, it.Id));
            }
            cs.Append("\n    public static readonly IReadOnlyDictionary<string, Item> ById = new Dictionary<string, Item>\n    {\n");
            foreach (var (ident, id) in idents)
            {
                cs.Append($"        [\"{id}\"] = {ident},\n");
            }
            cs.Append("    };\n\n    public static Item? FromId(string id) => ById.TryGetValue(id, out var item) ? item : null;\n");
            if (className == "Items")
            {
                Families(cs, "Item", families, idents.ToDictionary(p => p.Id, p => p.Ident, StringComparer.Ordinal));
            }
            cs.Append("}\n");
            return cs.ToString();
        }
    }
}

