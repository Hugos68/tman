using System;
using GBX.NET.Engines.Game;
using tman.src.Core;

namespace tman.src.Alterations.Internal
{
    public class InventoryRemoved(
        Predicate<CGameCtnBlock>? blockPredicate = null,
        Predicate<CGameCtnAnchoredObject>? itemPredicate = null) : Alteration
    {
        protected override void Run(CGameCtnChallenge map)
        {
            if (blockPredicate is not null)
            {
                map.Blocks?.RemoveAll(blockPredicate);
            }
            if (itemPredicate is not null)
            {
                map.AnchoredObjects?.RemoveAll(itemPredicate);
            }
        }
    }
}