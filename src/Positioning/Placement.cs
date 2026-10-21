
using System;
using System.Numerics;
using GBX.NET;
using GBX.NET.Engines.Game;

namespace tman.src.Positioning;

public static class Placement
{
    public static int GroundRow(CGameCtnChallenge map) =>
        map.Collection.ToString() switch
        {
            "RedIsland" or "WhiteShore" => 15,
            "BlueBay" or "GreenCoast" => 5,
            _ => map.DecoBaseHeightOffset
        };

    public static void PlaceRelative(
        CGameCtnChallenge map,
        CGameCtnBlock reference,
        CGameCtnBlock block,
        Vector3 offset = default)
    {
        throw new NotImplementedException();
    }

    public static void PlaceRelative(
        CGameCtnChallenge map,
        CGameCtnAnchoredObject reference,
        CGameCtnAnchoredObject item,
        Vector3 offset = default)
    {
        throw new NotImplementedException();
    }

    public static void PlaceRelative(
        CGameCtnChallenge map,
        CGameCtnBlock reference,
        CGameCtnAnchoredObject item,
        Vector3 offset = default)
    {
        throw new NotImplementedException();
    }

    public static void PlaceRelative(
        CGameCtnChallenge map,
        CGameCtnAnchoredObject reference,
        CGameCtnBlock block,
        Vector3 offset = default)
    {
        throw new NotImplementedException();
    }
}
