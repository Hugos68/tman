using GBX.NET.Engines.Game;
using tman.src.Core;

namespace tman.src.Alterations.Internal
{
    public class LightmapRemoved : Alteration
    {
        protected override void Run(CGameCtnChallenge map)
        {
            map.HasLightmaps = false;
            map.LightmapCache = null;
            map.LightmapFrames = null;
            map.LightmapCacheSmall = null;
            map.LightmapCacheData = null;
        }
    }
}