using System.Collections.Generic;
using System.Linq;
using GBX.NET;
using Tman.Core.Alterations.Internal;
using Tman.Core.Inventory;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Finishes
{
    public class StraightToTheCenter : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Straight to the Center")
        ];

        protected override void Run(Map map)
        {
            var finishes = map.Waypoints(Waypoint.Finish);
            if (finishes.Count == 0)
            {
                return;
            }
            var corners = finishes.SelectMany(finish => Corners(map, finish)).ToList();
            var centre = new Vec3(map.Size.X / 2 * MapConstants.CellSize, 0, map.Size.Z / 2 * MapConstants.CellSize);
            var offset = centre - new Vec3((corners.Min(c => c.X) + corners.Max(c => c.X)) / 2, 0, (corners.Min(c => c.Z) + corners.Max(c => c.Z)) / 2);
            foreach (var finish in finishes)
            {
                map.Translate(finish, offset);
            }
        }

        private static IEnumerable<Vec3> Corners(Map map, Placement finish)
        {
            var pose = finish.Pose(map);
            if (!finish.IsBlock)
            {
                return [pose.Coords];
            }
            var size = new Vec3((finish.BlockModel?.SizeX ?? 1) * MapConstants.CellSize, MapConstants.CellHeight, (finish.BlockModel?.SizeZ ?? 1) * MapConstants.CellSize);
            return new[] { 0f, size.X }.SelectMany(x => new[] { 0f, size.Y }.SelectMany(y => new[] { 0f, size.Z }.Select(z => new Vec3(x, y, z)))).Select(corner =>
            {
                var position = new Position(pose.Coords, pose.YawPitchRoll);
                position.Move(corner);
                return position.Coords;
            });
        }
    }
}