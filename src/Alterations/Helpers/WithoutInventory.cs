using System;
using GBX.NET.Engines.Game;

namespace tman.src.Alterations.Helpers;

public class WithoutInventory(
    Predicate<CGameCtnBlock>? blocks = null,
    Predicate<CGameCtnAnchoredObject>? items = null) : Alteration
{
    protected override void Run(CGameCtnChallenge map)
    {
        if (blocks is not null)
        {
            map.Blocks?.RemoveAll(blocks);
        }
        if (items is not null)
        {
            map.AnchoredObjects?.RemoveAll(items);
        }
    }
}