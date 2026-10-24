using System;
using GBX.NET.Engines.Game;
using tman.src.Core;

namespace tman.src.Alterations.Internal
{
    public class MapNameEdited(Func<string, string> edit) : Alteration
    {
        protected override void Run(CGameCtnChallenge map)
        {
            map.MapName = edit(map.MapName);
        }
    }
}