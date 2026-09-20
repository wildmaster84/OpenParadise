using OpenParadise.Fesl;
using OpenParadise.Game;
using OpenParadise.Net;

namespace OpenParadise.Handlers
{
    /// <summary>
    /// "+mgm" game-info push (used by gpsc and gset). Describes a game
    /// session to the client, including the per-participant OPPO/OPPARAM
    /// blocks.
    /// </summary>
    public static class Mgm
    {
        public static void SendMgm(FeslConnection conn, uint transactionId, GameSession game,
            GameSessionManager games, PlayerManager players)
        {
            var when = game.CreatedAt.ToString("yyyy.M.d-HH:mm:ss");
            var lines = new List<string>
            {
                $"IDENT={game.Id}",
                $"WHEN={when}",
                $"NAME={game.HostGamertag}",
                $"HOST=@{game.HostGamertag}",
                "ROOM=0",
                $"MAXSIZE={game.MaxSize}",
                $"MINSIZE={game.MinSize}",
                $"COUNT={game.Count}",
                $"PRIV={(game.Private ? 1 : 0)}",
                $"CUSTFLAGS={game.CustFlags}",
                $"SYSFLAGS={game.SysFlags}",
                "EVID=0",
                "EVGID=0",
                "NUMPART=1",
                $"SEED={game.Seed}",
                $"GPSHOST={game.HostGamertag}",
                "GPSREGION=0",
                "GAMEMODE=0",
                $"GAMEPORT={game.GamePort}",
                "VOIPPORT=0",
                $"WHENC={when}",
                $"SESS={conn.Player?.Session ?? "None"}",
                $"PLATPARAMS={game.PlatParams}",
                $"PARTSIZE0={game.MaxSize}",
                $"PARAMS={game.Params}",
                "PARTPARAMS0=",
            };

            var index = 0;
            foreach (var memberId in game.MemberIds)
            {
                var member = players.GetById(memberId);
                if (member == null)
                {
                    continue;
                }

                var isHost = memberId == game.HostPlayerId;
                lines.Add($"OPPO{index}=@{member.Gamertag}");
                lines.Add($"OPPART{index}=0");
                lines.Add($"OPFLAG{index}={(memberId == game.HostPlayerId ? 0 : game.CustFlags)}");
                lines.Add($"PRES{index}=0");
                lines.Add($"OPID{index}={member.Id}");
                lines.Add($"ADDR{index}={member.Address}");
                lines.Add($"LADDR{index}={member.Address}");
                lines.Add($"MADDR{index}={member.MacAddress}");
                lines.Add($"OPPARAM{index}={member.PlatParams}");
                index++;
            }

            conn.SendResponse("+mgm", transactionId, lines);
        }
    }
}
