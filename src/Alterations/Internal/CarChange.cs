
using System.Collections.Generic;
using GBX.NET.Engines.Game;
using tman.src.Core;

namespace tman.src.Alterations.Internal
{

    public enum Car
    {
        Desert,
        Rally,
        Snow,
        Sport
    }

    public class CarChange(Car car) : Alteration
    {
        private readonly Dictionary<Car, string> CarStrings = new()
        {
            { Car.Desert, "CarDesert" },
            { Car.Rally, "CarRally" },
            { Car.Snow, "CarSnow" },
            { Car.Sport, "CarSport" }
        };

        protected override void Run(CGameCtnChallenge map)
        {
            map.PlayerModel = new(CarStrings[car], 10003, "");
        }
    }
}