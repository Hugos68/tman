using System;
using System.Collections.Generic;
using System.Linq;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.GameData;
using Tman.Core.Catalog;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Maps
{
    public sealed partial class Map
    {
        private const string LinkedCheckpointTag = "LinkedCheckpoint";

        private static readonly Dictionary<Waypoint, string> Tags = new()
        {
            [Waypoint.Start] = "Spawn",
            [Waypoint.Checkpoint] = "Checkpoint",
            [Waypoint.Finish] = "Goal",
            [Waypoint.StartFinish] = "StartFinish",
        };

        internal void Link(IReadOnlyCollection<Placement> checkpoints)
        {
            if (checkpoints.Count < 2)
            {
                return;
            }
            var order = Placements.Where(placement => placement.IsWaypoint && placement.Property!.Tag == LinkedCheckpointTag).Select(placement => placement.Property!.Order).DefaultIfEmpty(0).Max() + 1;
            foreach (var checkpoint in checkpoints)
            {
                var property = checkpoint.Property ?? throw new InvalidOperationException($"{checkpoint.Id} is not a waypoint");
                property.Tag = LinkedCheckpointTag;
                property.Order = order;
            }
        }

        internal static CGameWaypointSpecialProperty? Property(CGameWaypointSpecialProperty? current, Waypoint kind)
        {
            if (kind == Waypoint.None)
            {
                return null;
            }
            var property = current;
            if (property is null)
            {
                property = new CGameWaypointSpecialProperty();
                property.CreateChunk<CGameWaypointSpecialProperty.Chunk2E009000>().Version = 2;
                property.CreateChunk<CGameWaypointSpecialProperty.Chunk2E009001>();
            }
            property.Tag = Tags[kind];
            return property;
        }
    }
}
