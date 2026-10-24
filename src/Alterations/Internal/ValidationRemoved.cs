using System.Xml.Linq;
using GBX.NET.Engines.Game;

namespace tman.src.Alterations.Internal
{
    public class ValidationRemoved : Alteration
    {
        protected override void Run(CGameCtnChallenge map)
        {
            map.AuthorTime = new(-1);
            map.GoldTime = new(-1);
            map.SilverTime = new(-1);
            map.BronzeTime = new(-1);
            map.ChallengeParameters?.RaceValidateGhost = null;
            if (map.Xml is not null)
                map.Xml = RemoveValidationFromXml(map.Xml);
        }

        private static string RemoveValidationFromXml(string xml)
        {
            var header = XElement.Parse(xml);
            header.Element("desc")?.SetAttributeValue("validated", "0");
            foreach (string time in new[] { "bronze", "silver", "gold", "authortime" })
            {
                header.Element("times")?.SetAttributeValue(time, "-1");
            }
            return header.ToString(SaveOptions.DisableFormatting);
        }
    }
}