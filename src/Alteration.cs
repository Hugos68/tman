using System.Collections.Generic;
using GBX.NET.Engines.Game;

namespace tman.src
{
    public abstract class Alteration
    {
        protected virtual IEnumerable<Alteration> PreappliedAlterations => [];

        public void Apply(CGameCtnChallenge map)
        {
            foreach (var alteration in PreappliedAlterations)
            {
                alteration.Apply(map);
            }
            Run(map);
        }

        protected virtual void Run(CGameCtnChallenge map) { }
    }
}