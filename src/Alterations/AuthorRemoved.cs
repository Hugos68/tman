
using TmEssentials;

namespace tman.src.Alterations
{
    public class AuthorRemoved : IAlteration
    {
        public void Apply(GBX.NET.Engines.Game.CGameCtnChallenge map)
        {
            map.AuthorExtraInfo = string.Empty;
            map.AuthorLogin = string.Empty;
            map.AuthorNickname = string.Empty;
            map.AuthorScore = 0;
            map.AuthorTime = new TimeInt32(0);
            map.AuthorVersion = 0;
            map.AuthorZone = string.Empty;
        }
    }
}