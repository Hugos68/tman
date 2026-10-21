using GBX.NET.Engines.Game;
using tman.src;

namespace tman.src.Alterations
{
    public class Freewheel : IAlteration
    {
        public void Apply(CGameCtnChallenge map)
        {
            new StartEffect("NoEngine", new(0, 0, 0)).Apply(map);
            new StartEffect("Turbo", new(0, 0, 0)).Apply(map);
            new StartEffect("NoEngine", new(0, 0, 2)).Apply(map);
            new CheckpointEffect("NoEngine", new(0, 0, -2)).Apply(map);
            new CheckpointEffect("Turbo", new(0, 0, 0)).Apply(map);
            new CheckpointEffect("NoEngine", new(0, 0, 2)).Apply(map);
        }
    }
}