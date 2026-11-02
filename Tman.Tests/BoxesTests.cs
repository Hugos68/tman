using System.Linq;
using GBX.NET;
using Tman.Core.Maps.Geometry;
using Xunit;

namespace Tman.Tests
{
    public class BoxesTests
    {
        [Fact]
        public void CellsInARowBecomeOneBoxPerRun()
        {
            var boxes = Boxes.OfCells([(0, 0), (1, 0), (2, 0), (4, 0), (0, 1)], 32, 1, 2);
            Assert.Equal(
            [
                new Box(0, 96, 0, 32, 1, 2),
                new Box(128, 160, 0, 32, 1, 2),
                new Box(0, 32, 32, 64, 1, 2),
            ], boxes);
        }

        [Fact]
        public void MergeJoinsNeighboursAlongBothAxesUntilNothingMoves()
        {
            var merged = Boxes.Merge([new Box(0, 32, 0, 32, 0, 1), new Box(32, 64, 0, 32, 0, 1), new Box(0, 32, 32, 64, 0, 1), new Box(32, 64, 32, 64, 0, 1)]);
            Assert.Equal([new Box(0, 64, 0, 64, 0, 1)], merged);
        }

        [Fact]
        public void MergeKeepsBoxesOfDifferentHeightsApart()
        {
            var merged = Boxes.Merge([new Box(0, 32, 0, 32, 0, 1), new Box(32, 64, 0, 32, 0, 2)]);
            Assert.Equal(2, merged.Count);
        }

        [Fact]
        public void ABoxHasEightCornersBottomFirst()
        {
            var corners = new Box(0, 1, 0, 2, 3, 4).Corners();
            Assert.Equal(8, corners.Length);
            Assert.Equal(4, corners.Count(corner => corner.Y == 3));
            Assert.Contains(new Vec3(1, 4, 2), corners);
        }

        [Theory]
        [InlineData(Direction.North, 0, 0, 0, 0)]
        [InlineData(Direction.East, 0, 0, 1, 0)]
        [InlineData(Direction.South, 0, 0, 2, 1)]
        [InlineData(Direction.West, 0, 0, 0, 2)]
        public void ADirectionTurnsAFootprintCellInsideItsSize(Direction direction, int x, int z, int turnedX, int turnedZ)
        {
            Assert.Equal((turnedX, turnedZ), direction.Turn((x, z), (3, 2)));
        }
    }
}
