using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tman.Generator
{
    internal static partial class NamingRules
    {
        private const string Penalty = "Penalty";

        private const string Straight = "Straight";

        private const string Platform = "Platform";

        private const string GateSpecial = "GateSpecial";

        private const string ItemExtension = ".Item.Gbx";

        private const string Stadium = "Stadium";

        private static readonly string[] PenaltySurfaces = ["", "Dirt", "Ice"];

        private const string WallCheckpoint = "WallCheckpoint";

        private const string WithWall = "WithWall";

        private static readonly (string Suffix, int QuarterTurns)[] PlainTurns =
            [("Slope2Down", 2), ("Slope2Left", 1), ("Slope2Right", 3), ("CheckpointSlopeDown", 2), ("CheckpointTiltLeft", 2), ("WithWallCheckpointRight", 2)];

        private static readonly (string Waypoint, string Replacement)[] SpecialGateEdits =
            [("Start", "Special"), ("Checkpoint", "Special"), ("Finish", "Special"), ("Left", ""), ("Center", ""), ("Right", ""), ("v2", "")];

        [GeneratedRegex("^Platform([A-Z][a-z]+)Base$")]
        private static partial Regex PlatformFamilyBase();

        public static int PlainQuarterTurns(string checkpointId)
        {
            if (checkpointId.Contains(WallCheckpoint, StringComparison.Ordinal) && !checkpointId.Contains(WithWall, StringComparison.Ordinal))
            {
                return 1;
            }
            return PlainTurns.FirstOrDefault(turn => checkpointId.EndsWith(turn.Suffix, StringComparison.Ordinal)).QuarterTurns;
        }

        public static SortedDictionary<string, string> PlainOfPenalty(IReadOnlyCollection<string> blockIds)
        {
            var ids = blockIds.ToHashSet(StringComparer.Ordinal);
            var plains = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var id in blockIds)
            {
                var index = id.IndexOf(Penalty, StringComparison.Ordinal);
                if (index < 0 || Array.IndexOf(PenaltySurfaces, id[(index + Penalty.Length)..]) < 0)
                {
                    continue;
                }
                var plain = id[..index] + Straight;
                if (ids.Contains(plain))
                {
                    plains[id] = plain;
                }
            }
            return plains;
        }

        public static SortedDictionary<string, string> PlatformFamilyOf(IReadOnlyCollection<string> blockIds)
        {
            var families = blockIds.Select(id => PlatformFamilyBase().Match(id)).Where(match => match.Success).Select(match => match.Groups[1].Value).OrderByDescending(family => family.Length).ToList();
            var familyOf = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var id in blockIds)
            {
                if (families.FirstOrDefault(family => id.StartsWith(Platform + family, StringComparison.Ordinal)) is { } family)
                {
                    familyOf[id] = family;
                }
            }
            return familyOf;
        }

        public static SortedDictionary<(string Block, string Family), string> PlatformTwins(IReadOnlyCollection<string> blockIds)
        {
            var ids = blockIds.ToHashSet(StringComparer.Ordinal);
            var familyOf = PlatformFamilyOf(blockIds);
            var families = familyOf.Values.Distinct(StringComparer.Ordinal).ToList();
            var twins = new SortedDictionary<(string, string), string>();
            foreach (var (id, family) in familyOf)
            {
                var shape = id[(Platform.Length + family.Length)..];
                foreach (var other in families.Where(other => other != family))
                {
                    if (ids.Contains(Platform + other + shape))
                    {
                        twins[(id, other)] = Platform + other + shape;
                    }
                }
            }
            return twins;
        }

        public static SortedDictionary<(string Item, string Gate), string> SpecialGates(IReadOnlyCollection<string> blockIds, IReadOnlyDictionary<string, int?> itemWaypoints)
        {
            var gates = new SortedDictionary<(string, string), string>();
            foreach (var gate in blockIds.Where(id => id.StartsWith(GateSpecial, StringComparison.Ordinal)))
            {
                var effect = gate[GateSpecial.Length..];
                foreach (var (item, waypoint) in itemWaypoints.Where(pair => pair.Value is not null))
                {
                    var special = SpecialGateEdits.Aggregate(item, (id, edit) => id.Replace(edit.Waypoint, edit.Replacement, StringComparison.Ordinal)) + effect;
                    if (itemWaypoints.ContainsKey(special))
                    {
                        gates[(item, gate)] = special;
                    }
                }
            }
            return gates;
        }

        public static SortedDictionary<(string Item, string Collection), string> Skinned(IReadOnlyCollection<string> itemIds, IEnumerable<string> collections)
        {
            var ids = itemIds.ToHashSet(StringComparer.Ordinal);
            var skinned = new SortedDictionary<(string, string), string>();
            foreach (var collection in collections.Where(collection => collection != Stadium))
            {
                var suffix = collection + ItemExtension;
                foreach (var id in itemIds.Where(id => id.EndsWith(suffix, StringComparison.Ordinal)))
                {
                    var plain = id[..^suffix.Length] + ItemExtension;
                    if (ids.Contains(plain))
                    {
                        skinned[(plain, collection)] = id;
                    }
                }
            }
            return skinned;
        }
    }
}
