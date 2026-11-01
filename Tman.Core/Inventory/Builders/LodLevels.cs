using System.Collections.Generic;
using System.Linq;

namespace Tman.Core.Inventory.Builders
{
    public static class LodLevels
    {
        public static float[] Merge(IEnumerable<float[]> distanceLists) => [.. distanceLists.SelectMany(distances => distances).Distinct().Order()];

        public static int Remap(int mask, float[] distances, float[] merged)
        {
            var result = 0;
            for (var level = 0; level <= merged.Length; level++)
            {
                var start = level == 0 ? 0 : merged[level - 1];
                var sourceLevel = distances.TakeWhile(distance => distance <= start).Count();
                if ((mask & (1 << sourceLevel)) != 0)
                {
                    result |= 1 << level;
                }
            }
            return result;
        }
    }
}
