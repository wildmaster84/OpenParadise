using OpenParadise.Fesl;
using OpenParadise.Net;

namespace OpenParadise.Handlers
{
    /// <summary>+who player-info push (used by news, gpsc, hchk, gdel).</summary>
    public static class Who
    {
        /// <summary>
        /// Sends a +who push. gameId mirrors the verified Python reference:
        /// 0 = lobby, 73 = in-game (hchk), 76 = joining (gpsc).
        /// </summary>
        public static void SendWho(FeslConnection conn, uint transactionId, uint gameId)
        {
            var player = conn.Player;
            if (player == null)
            {
                return;
            }

            conn.SendResponse("+who", transactionId, new[]
            {
                "I=947",
                "N=" + player.Gamertag,
                "M=" + player.Gamertag,
                "F=U",
                "A=" + conn.RemoteIp,
                "P=1",
                "S=,,",
                "G=" + gameId,
                "AT=",
                "CL=511",
                "LV=1049601",
                "MD=0",
                "LA=" + conn.RemoteIp,
                "HW=0",
                "RP=0",
                "MA=" + player.MacAddress,
                "LO=enUS",
                "X=",
                "US=0",
                "PRES=1",
                "VER=7",
                "C=,,,,,,,,",
            });
        }
    }
}
