using GBX.NET.Engines.Game;

namespace tman.src
{
    public static class AlterationRunner
    {
        public static void Run(
            CGameCtnChallenge map,
            params Alteration[] alterations)
        {
            foreach (Alteration alteration in alterations)
            {
                alteration.Apply(map);
            }
        }
    }
}