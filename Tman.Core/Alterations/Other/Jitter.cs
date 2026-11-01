using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using GBX.NET;
using Tman.Core.Maps;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Alterations.Other
{
    public class Jitter : StandardAlteration
    {
        private const float Shift = 0.25f;

        private const float Tilt = MathF.PI / 45;

        protected override string DisplayName => "Jitter";

        protected override void Run(Map map)
        {
            foreach (var placement in map.Placements.Where(p => !p.IsWaypoint))
            {
                if (placement.IsBlock)
                {
                    if (placement.BlockModel is not { Air: true })
                    {
                        continue;
                    }
                    map.Free(placement);
                }
                var pose = placement.Pose(map);
                var random = new Random(Seed($"{map.Name}|{placement.Id}|{pose.Coords}"));
                MapMoving.Nudge(placement, new Vec3(Spread(random, Shift), Spread(random, Shift), Spread(random, Shift)), new Vec3(Spread(random, Tilt), Spread(random, Tilt), Spread(random, Tilt)));
            }
        }

        private static float Spread(Random random, float extent) => (float)(random.NextDouble() * 2 - 1) * extent;

        private static int Seed(string key) => BitConverter.ToInt32(SHA256.HashData(Encoding.UTF8.GetBytes(key)), 0);
    }
}
