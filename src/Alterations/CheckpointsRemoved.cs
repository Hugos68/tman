namespace tman.src.Alterations
{
    public class CheckpointsRemoved : IAlteration
    {
        public void Apply(GBX.NET.Engines.Game.CGameCtnChallenge map)
        {
            map.Blocks.RemoveAll(block => block.BlockModel.Id.Contains("Checkpoint"));
            map.AnchoredObjects.RemoveAll(obj => obj.ItemModel.Id.Contains("Checkpoint"));
            map.MapName += " Checkpoints Removed";
        }
    }
}