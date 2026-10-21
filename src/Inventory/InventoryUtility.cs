using System;
using GBX.NET.Engines.Game;

namespace tman.src.Inventory
{
    public static class InventoryUtility
    {
        public static bool IsWaypoint(CGameCtnBlock block) => block.WaypointSpecialProperty is not null;
        public static bool IsWaypoint(CGameCtnAnchoredObject  item) => item.WaypointSpecialProperty is not null;
        public static bool IsCheckpoint(CGameCtnBlock block) => IsWaypoint(block) && block.BlockModel.Id.Contains("Checkpoint");
        public static bool IsCheckpoint(CGameCtnAnchoredObject item) => IsWaypoint(item) && item.ItemModel.Id.Contains("Checkpoint");
        public static bool IsStart(CGameCtnBlock block) => IsWaypoint(block) && block.BlockModel.Id.Contains("Start");
        public static bool IsStart(CGameCtnAnchoredObject item) => IsWaypoint(item) && item.ItemModel.Id.Contains("Start");
        public static string ItemCheckpointIdToItemEffectId(string id, string effect)
        {
            return id
                .Replace("Center", "", StringComparison.Ordinal)
                .Replace("Left", "", StringComparison.Ordinal)
                .Replace("Right", "", StringComparison.Ordinal)
                .Replace("v2", "", StringComparison.Ordinal)
                .Replace("Checkpoint", "Special", StringComparison.Ordinal)
                + effect;
        }
    }
}