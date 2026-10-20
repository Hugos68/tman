using GBX.NET.Engines.Game;

namespace tman.src
{
    public class AlterationRunner(CGameCtnChallenge map)
    {
        private CGameCtnChallenge Map { get; } = map;
        
        public void Run(
            params IAlteration[] alterations)
        {
            foreach (IAlteration alteration in alterations)
            {
                alteration.Apply(Map);
            }
        }
    }
}