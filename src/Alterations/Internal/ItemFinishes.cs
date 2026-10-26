using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GBX.NET.Engines.Game;
using tman.src.Core;
using tman.src.Inventory;
using tman.src.Positioning;

namespace tman.src.Alterations.Internal
{
    public class ItemFinishes(IReadOnlyDictionary<string, string> finishes, Predicate<CGameCtnBlock>? blockPredicate = null, Predicate<CGameCtnAnchoredObject>? itemPredicate = null) : Alteration
    {
        private static readonly Regex VariantRegex = new(@"V(\d+)$", RegexOptions.Compiled);

        protected override void Run(CGameCtnChallenge map)
        {
            foreach (var (sourceId, finishPath) in finishes)
            {
                var match = VariantRegex.Match(sourceId);
                var model = match.Success ? sourceId[..match.Index] : sourceId;
                int? variant = match.Success ? int.Parse(match.Groups[1].Value) : null;

                var items = (map.AnchoredObjects ?? [])
                    .Where(item => itemPredicate is null || itemPredicate(item))
                    .Where(item => item.ItemModel.Id == model && (variant is null || Variant(item) == variant))
                    .ToList();
                var blocks = (map.Blocks ?? [])
                    .Where(block => blockPredicate is null || blockPredicate(block))
                    .Where(block => block.BlockModel.Id == model && (variant is null || Variant(block) == variant))
                    .ToList();
                if (items.Count == 0 && blocks.Count == 0)
                {
                    continue;
                }
                EmbedUtility.Embed(map, finishPath);
                var finish = new CGameCtnAnchoredObject
                {
                    ItemModel = EmbedUtility.ExtractIdent(finishPath)
                };
                foreach (var item in items)
                {
                    MapUtility.PlaceRelative(map, item, finish);
                }
                foreach (var block in blocks)
                {
                    MapUtility.PlaceRelative(map, block, finish);
                }
            }
        }

        private static int Variant(CGameCtnAnchoredObject item) => (item.Flags >> 8) & 0xFF;

        private static int Variant(CGameCtnBlock block) => (block.Flags & 0x3F) + 64 * ((block.Flags >> 21) & 0x7F);
    }
}
