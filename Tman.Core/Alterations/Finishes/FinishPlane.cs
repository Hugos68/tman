using System.Collections.Generic;
using System.Linq;
using Tman.Core.Game;
using Tman.Core.Maps;
using Tman.Core.Maps.Geometry;

namespace Tman.Core.Alterations.Finishes
{
    internal static class FinishPlane
    {
        private const string Template = "FinishPlane.Item.Gbx";

        public static void Place(Map map, AlterationContext context, string name, IReadOnlyList<Box> boxes, bool visible)
        {
            using var template = context.Open(Assets.Template(Template));
            var ident = map.Ident(name);
            map.PlaceEmbedded(ident, ModelerItem.Build(template, ident, [.. Boxes.Merge(boxes).Select(box => box.Corners())], visible));
        }
    }
}
