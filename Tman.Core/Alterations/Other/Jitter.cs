using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using GBX.NET;
using Tman.Core.Alterations.Internal;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Other
{
    public class Jitter : Alteration
    {
        private const float Shift = 0.5f;

        private const float Tilt = MathF.PI / 45;

        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new MapNameEdited(name => name + " Jitter")
        ];

        protected override void Run(Map map)
        {
            foreach (var placement in map.Placements.ToList())
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
                Nudge(placement, new Vec3(Spread(random, Shift), Spread(random, Shift), Spread(random, Shift)), new Vec3(Spread(random, Tilt), Spread(random, Tilt), Spread(random, Tilt)));
            }
        }

        private static void Nudge(Placement placement, Vec3 offset, Vec3 yawPitchRoll)
        {
            if (placement.Block is { } block)
            {
                block.AbsolutePositionInMap += offset;
                block.YawPitchRoll += yawPitchRoll;
                return;
            }
            placement.Item!.AbsolutePositionInMap += offset;
            placement.Item.YawPitchRoll += yawPitchRoll;
        }


        private static float Spread(Random random, float extent) => (float)(random.NextDouble() * 2 - 1) * extent;
        private static int Seed(string key) => BitConverter.ToInt32(SHA256.HashData(Encoding.UTF8.GetBytes(key)), 0);
    }
}