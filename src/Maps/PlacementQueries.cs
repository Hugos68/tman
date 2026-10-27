using System.Collections.Generic;
using System.Linq;
using tman.src.Core;

namespace tman.src.Maps
{
    public static class PlacementQueries
    {
        public static List<Placement> Of(this IEnumerable<Placement> placements, Selector selector) =>
            placements.Where(selector.Matches).ToList();

        public static List<Placement> Of(this IEnumerable<Placement> placements, params Block[] blocks) =>
            placements.Where(placement => placement.IsBlock && blocks.Any(block => block.Id == placement.Id)).ToList();

        public static IEnumerable<Placement> Ground(this IEnumerable<Placement> placements) => placements.Where(placement => placement.IsGround);

        public static IEnumerable<Placement> OnlyBlocks(this IEnumerable<Placement> placements) => placements.Where(placement => placement.IsBlock);

        public static IEnumerable<Placement> OnlyItems(this IEnumerable<Placement> placements) => placements.Where(placement => !placement.IsBlock);
    }
}
