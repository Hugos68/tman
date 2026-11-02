using System;
using System.Collections.Generic;
using System.Linq;

namespace Tman.Core.Maps.Geometry
{
    internal static class Boxes
    {
        public static List<Box> OfCells(IEnumerable<(int X, int Z)> cells, float cellSize, float y0, float y1)
        {
            var boxes = new List<Box>();
            foreach (var row in cells.Distinct().GroupBy(cell => cell.Z))
            {
                foreach (var (x0, x1) in Runs(row.Select(cell => cell.X)))
                {
                    boxes.Add(new Box(x0 * cellSize, x1 * cellSize, row.Key * cellSize, (row.Key + 1) * cellSize, y0, y1));
                }
            }
            return boxes;
        }

        public static List<Box> Merge(IReadOnlyList<Box> boxes)
        {
            var list = boxes.ToList();
            while (true)
            {
                var count = list.Count;
                list.Sort((a, b) => (a.Y0, a.Y1, a.X0, a.X1, a.Z0).CompareTo((b.Y0, b.Y1, b.X0, b.X1, b.Z0)));
                list = MergeRuns(list, b => (b.Y0, b.Y1, b.X0, b.X1), b => b.Z0, b => b.Z1, (a, b) => a with { Z1 = b.Z1 });
                list.Sort((a, b) => (a.Y0, a.Y1, a.Z0, a.Z1, a.X0).CompareTo((b.Y0, b.Y1, b.Z0, b.Z1, b.X0)));
                list = MergeRuns(list, b => (b.Y0, b.Y1, b.Z0, b.Z1), b => b.X0, b => b.X1, (a, b) => a with { X1 = b.X1 });
                if (list.Count == count)
                {
                    return list;
                }
            }
        }

        private static IEnumerable<(int Start, int End)> Runs(IEnumerable<int> values)
        {
            var sorted = values.Distinct().Order().ToList();
            for (var i = 0; i < sorted.Count;)
            {
                var start = sorted[i];
                var end = start;
                while (i + 1 < sorted.Count && sorted[i + 1] == end + 1)
                {
                    i++;
                    end = sorted[i];
                }
                i++;
                yield return (start, end + 1);
            }
        }

        private static List<Box> MergeRuns<TKey>(List<Box> sorted, Func<Box, TKey> key, Func<Box, float> start, Func<Box, float> end, Func<Box, Box, Box> join) where TKey : IEquatable<TKey>
        {
            var result = new List<Box>(sorted.Count);
            foreach (var box in sorted)
            {
                if (result.Count > 0 && key(result[^1]).Equals(key(box)) && end(result[^1]) == start(box))
                {
                    result[^1] = join(result[^1], box);
                    continue;
                }
                result.Add(box);
            }
            return result;
        }
    }
}
