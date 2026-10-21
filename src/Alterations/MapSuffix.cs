using GBX.NET.Engines.Game;

namespace tman.src.Alterations
{
    public class MapNameSuffix(string suffix) : IAlteration
    {
        public void Apply(CGameCtnChallenge map)
        {
            map.MapName += suffix;
        }
    }
}