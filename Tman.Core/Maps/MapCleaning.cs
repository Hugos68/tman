using System.Xml.Linq;

namespace Tman.Core.Maps
{
    public static class MapCleaning
    {
        public static void RemoveValidation(this Map map)
        {
            var gbx = map.Gbx;
            gbx.AuthorTime = new(-1);
            gbx.GoldTime = new(-1);
            gbx.SilverTime = new(-1);
            gbx.BronzeTime = new(-1);
            gbx.ChallengeParameters?.RaceValidateGhost = null;
            if (gbx.Xml is not null)
            {
                var header = XElement.Parse(gbx.Xml);
                header.Element("desc")?.SetAttributeValue("validated", "0");
                foreach (var time in new[] { "bronze", "silver", "gold", "authortime" })
                {
                    header.Element("times")?.SetAttributeValue(time, "-1");
                }
                gbx.Xml = header.ToString(SaveOptions.DisableFormatting);
            }
        }

        public static void RemoveLightmap(this Map map)
        {
            var gbx = map.Gbx;
            gbx.HasLightmaps = false;
            gbx.LightmapCache = null;
            gbx.LightmapFrames = null;
            gbx.LightmapCacheSmall = null;
            gbx.LightmapCacheData = null;
        }
    }
}
