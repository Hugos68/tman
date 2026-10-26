using System;
using GBX.NET.Engines.Game;
using tman.src.Core;

namespace tman.src.Alterations.Internal
{
    public class ConditionalAlteration(Predicate<CGameCtnChallenge> predicate, Alteration alteration) : Alteration
    {
        protected override void Run(CGameCtnChallenge map)
        {
            if (predicate(map))
            {
                alteration.Apply(map);
            }
        }

    }
}