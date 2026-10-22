using GBX.NET.Engines.Game;

namespace tman.src.Alterations.Helpers
{
    public class MapNameSuffix(string suffix) : Alteration
    {
        protected override void Run(CGameCtnChallenge map)
        {
            map.MapName += suffix;
        }
    }
}