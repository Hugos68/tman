using GBX.NET.Engines.Game;

namespace tman.src
{
    public static class AlterationRunner
    {
        public static void Run(
            CGameCtnChallenge map,
            params IAlteration[] alterations)
        {
            foreach (IAlteration alteration in alterations)
            {
                alteration.Apply(map);
            }
        }
    }
}