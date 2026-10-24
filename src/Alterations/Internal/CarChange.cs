
using System;
using GBX.NET.Engines.Game;

namespace tman.src.Alterations.Internal
{
    public class CarChange(Car car) : Alteration
    {
        protected override void Run(CGameCtnChallenge map)
        {
            map.PlayerModel = new(car switch
            {
                Car.Desert => "CarDesert",
                Car.Rally => "CarRally",
                Car.Snow => "CarSnow",
                Car.Sport => "CarSport",
                _ => throw new ArgumentException($"Unknown car: {car}")
            }, 10003, "");
        }
    }

    public enum Car
    {
        Desert,
        Rally,
        Snow,
        Sport
    }
}