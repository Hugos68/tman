using System;
using System.Linq;

namespace Tman.Core.Surfaces
{
    internal readonly record struct Tint(float Red, float Green, float Blue)
    {
        public static readonly Tint None = new(-1, -1, -1);

        public int[] Constants => this == None ? [] : [.. new[] { Red, Green, Blue }.Select(BitConverter.SingleToInt32Bits)];
    }
}
