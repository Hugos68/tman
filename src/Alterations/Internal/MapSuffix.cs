using GBX.NET.Engines.Game;

namespace tman.src.Alterations.Internal
{
    public class MapNameSuffix(string suffix) : Alteration
    {
        protected override void Run(CGameCtnChallenge map)
        {
            map.MapName += suffix;
        }
    }
}