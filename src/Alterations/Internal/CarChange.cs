
using GBX.NET.Engines.Game;

namespace tman.src.Alterations.Internal
{
    public class CarChange(string car) : Alteration
    {
        protected override void Run(CGameCtnChallenge map)
        {
            map.PlayerModel = new(car, 10003, "");
        }
    }
}