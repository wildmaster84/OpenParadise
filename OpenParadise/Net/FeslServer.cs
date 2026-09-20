using System.Net;
using System.Net.Sockets;
using OpenParadise.Data;
using OpenParadise.Fesl;
using OpenParadise.Game;

namespace OpenParadise.Net
{
    /// <summary>
    /// TCP listener + connection accept loop + command dispatch.
    /// </summary>
    public class FeslServer
    {
        private Socket? _listener;
        private readonly List<FeslConnection> _connections = new();
        private readonly object _connLock = new();

        public int Port { get; }
        public PlayerManager Players { get; } = new();
        public GameSessionManager Games { get; } = new();

        public static bool Debug { get; set; } = true;

        public FeslServer(int port)
        {
            Port = port;
        }

        public void Start()
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(new IPEndPoint(IPAddress.Any, Port));
            _listener.Listen(16);
            Console.WriteLine($"[FESL] listening on port {Port}");
            _ = AcceptLoopAsync();
        }

        private async Task AcceptLoopAsync()
        {
            while (true)
            {
                Socket client;
                try
                {
                    client = await _listener!.AcceptAsync();
                }
                catch (Exception)
                {
                    break;
                }

                var connection = new FeslConnection(client, Players, Games);
                lock (_connLock)
                {
                    _connections.Add(connection);
                }
                connection.Disconnected += c =>
                {
                    lock (_connLock)
                    {
                        _connections.Remove(c);
                    }
                };
                _ = connection.RunAsync();
            }
        }

        /// <summary>
        /// Central command dispatch. Called by each connection for every
        /// complete request packet.
        /// </summary>
        public static Task DispatchAsync(FeslConnection conn, FeslPacket packet,
            IReadOnlyDictionary<string, string> data)
        {
            var command = packet.Command;
            var transactionId = packet.TransactionId;
            if (Debug && command != "~png")
            {
                var preview = string.Join(", ",
                    data.Take(6).Select(kv => $"{kv.Key}={kv.Value}"));
                conn.Log($"[REQ] {command} TXN={transactionId:X6} {preview}");
            }

            switch (command)
            {
                case "@tic":
                    return Task.CompletedTask; // no response

                case "@dir":
                    // Verified wire format: body "ADDR=127.0.0.0\tPORT=10134\t
                    // MASK=ffff<pad>\tSESS=1\0" padded to exactly 88 bytes
                    // total (the old C# PadMask trick). The game opens its
                    // real session against ADDR:PORT.
                    var body = System.Text.Encoding.ASCII
                        .GetBytes($"ADDR={Config.ServerIp}\tPORT={Config.ServerPort}\tMASK=ffff\tSESS=1\0");
                    var pad = 88 - FeslPacket.HeaderSize - body.Length;
                    if (pad > 0)
                    {
                        // Grow the MASK value with 'f' padding.
                        body = System.Text.Encoding.ASCII
                            .GetBytes($"ADDR={Config.ServerIp}\tPORT={Config.ServerPort}\tMASK=ffff" +
                                      new string('f', pad) + "\tSESS=1\0");
                    }
                    conn.SendResponse("@dir", transactionId, body);
                    return Task.CompletedTask;

                case "addr":
                    HandleAddr(conn, data);
                    return Task.CompletedTask;

                case "skey":
                    HandleSkey(conn, transactionId, data);
                    return Task.CompletedTask;

                case "auth":
                    HandleAuth(conn, transactionId, data);
                    return Task.CompletedTask;

                case "pers":
                    HandlePers(conn, transactionId, data, packet);
                    return Task.CompletedTask;

                case "news":
                    HandleNews(conn, transactionId, data);
                    return Task.CompletedTask;

                case "sele":
                    HandleSele(conn, transactionId, data);
                    return Task.CompletedTask;

                case "usld":
                    conn.SendResponse("usld", transactionId, new[]
                    {
                        "SPM_EA=1",
                        "SPM_PART=0",
                        "IMGATE=0",
                        $"UID={conn.Player?.PersonaId ?? "0"}",
                        "QMSG0=\"Wanna play?\"",
                        "QMSG1=\"I rule!\"",
                        "QMSG2=Doh!",
                        "QMSG3=\"Mmmm... doughnuts.\"",
                        "QMSG4=\"What time is it?\"",
                        "QMSG5=\"The truth is out of style.\"",
                    });
                    return Task.CompletedTask;

                case "fget":
                    conn.SendResponse("fget", transactionId, Array.Empty<string>());
                    return Task.CompletedTask;

                case "fupd":
                    if (conn.Player != null && data.TryGetValue("ADD", out var friends))
                    {
                        conn.Player.Friends = friends;
                        conn.SendResponse("fupd", transactionId, new[] { friends });
                    }
                    else
                    {
                        conn.SendResponse("fupd", transactionId, Array.Empty<string>());
                    }
                    return Task.CompletedTask;

                case "slst":
                    conn.SendResponse("slst", transactionId, StaticData.StatsViews);
                    return Task.CompletedTask;

                case "sviw":
                    conn.SendResponse("sviw", transactionId, StaticData.StatsViewInfo);
                    return Task.CompletedTask;

                case "sdta":
                    conn.SendResponse("sdta", transactionId, new[]
                    {
                        "SLOT=0",
                        "STATS=1,2,3,4,5,6,7,8,9,10,11,12,13",
                    });
                    return Task.CompletedTask;

                case "gpsc":
                    HandleGpsc(conn, transactionId, data);
                    return Task.CompletedTask;

                case "gset":
                    HandleGset(conn, transactionId, data);
                    return Task.CompletedTask;

                case "gdel":
                    HandleGdel(conn, transactionId);
                    return Task.CompletedTask;

                case "gqwk":
                    // Quick-join: no games to match -> "not found" (txn 'fnd').
                    conn.Send(FeslWriter.Build("gqwk", FeslPacketType.Unknown6E,
                        0x00666E64, FeslWriter.BuildBody(Array.Empty<string>())));
                    return Task.CompletedTask;

                case "hchk":
                    conn.SendResponse("hchk", transactionId, Array.Empty<string>());
                    Handlers.Who.SendWho(conn, transactionId, 73);
                    return Task.CompletedTask;

                case "rent":
                    conn.SendResponse("rent", transactionId, new[]
                    {
                        "CALLUSER=947",
                        "GFIDS=0",
                    });
                    return Task.CompletedTask;

                case "cate":
                    // Leaderboard category: valid-but-empty definition so the
                    // UI shows an empty board instead of hanging (parse order
                    // from the game's cate callback: SS, SYMS, VIEW%d rows,
                    // CC/IC/VC, R/U, CHAN, SEQN, RANGE, DESC).
                    conn.SendResponse("cate", transactionId, new[]
                    {
                        "SS=1",
                        "SYMS=RANK",
                        "CC=1",
                        "IC=1",
                        "VC=1",
                        "R=",
                        "U=",
                        "CHAN=0",
                        "SEQN=0",
                        "RANGE=0-0",
                        "DESC=Leaderboard",
                    });
                    return Task.CompletedTask;

                case "gsea":
                case "snap":
                case "rrup":
                case "opup":
                case "fbst":
                case "rrgt":
                case "rrlc":
                    conn.SendResponse(command, transactionId, Array.Empty<string>());
                    return Task.CompletedTask;

                default:
                    if (command != "~png")
                    {
                        conn.Log($"[FESL] unhandled command '{command}' (TXN {transactionId:X6}) - sending empty ack");
                    }
                    // Catch-all: acknowledge any unhandled request with an
                    // empty body so its pending entry pops. An unanswered
                    // request wedges the game's entry matcher and the
                    // session watchdog kills the connection.
                    conn.SendResponse(command, transactionId, Array.Empty<string>());
                    return Task.CompletedTask;
            }
        }

        private static void HandleAddr(FeslConnection conn,
            IReadOnlyDictionary<string, string> data)
        {
            if (conn.Player == null)
            {
                conn.Player = conn.Players.Create("");
            }
            conn.Player.Address = FeslPacket.Get(data, "ADDR") ?? conn.RemoteIp;
            _ = int.TryParse(FeslPacket.Get(data, "PORT"), out var port);
            conn.Player.Port = port;
            // Python reference starts the heartbeat as soon as the client
            // registers its address.
            conn.StartHeartbeat();
        }

        private static void HandleSkey(FeslConnection conn, uint transactionId,
            IReadOnlyDictionary<string, string> data)
        {
            if (conn.Player == null)
            {
                conn.Player = conn.Players.Create("");
            }
            conn.Player.SessionKey = FeslPacket.Get(data, "SKEY") ?? "";
            conn.SendResponse("skey", transactionId, new[]
            {
                "SKEY=$baadcodebaadcodebaadcodebaadcode",
                "DP=XBL2/Burnout-Jan2008/mod",
            });

            // The working server pushes an unsolicited news packet with the
            // full client.cfg config (txn 0, type 0x00) immediately after
            // the skey response - the game expects it before proceeding.
            var cfgBody = FeslWriter.BuildBody(StaticData.ClientConfig);
            conn.Send(FeslWriter.Build("news", FeslPacketType.Ping, 0, cfgBody));
        }

        private static void HandleAuth(FeslConnection conn, uint transactionId,
            IReadOnlyDictionary<string, string> data)
        {
            var player = conn.Player ??= conn.Players.Create("");
            player.Xuid = FeslPacket.Get(data, "XUID") ?? "";
            player.Gamertag = FeslPacket.Get(data, "GTAG") ?? "";
            player.PersonaId = FeslPacket.Get(data, "MID") ?? "";
            player.MacAddress = FeslPacket.Get(data, "MADDR") ?? "";
            player.Address ??= conn.RemoteIp;

            Console.WriteLine($"{player.Gamertag} (XUID {player.Xuid}) authenticated");

            // _LUID must be LITERAL ASCII "_LUID=$0000000000000757" on the
            // wire (the writer's $-hex decoding must NOT touch it), so the
            // body is built with a raw bytes line. ADDR uses the TCP peer
            // IP (127.0.0.1 under Xenia), not the game's reported WAN addr.
            var authBody = FeslWriter.BuildBody(new[]
            {
                "LAST=2018.1.1-00:00:00",
                "TOS=1",
                "SHARE=1",
                "NAME=" + player.Gamertag,
                "PERSONAS=" + player.Gamertag,
                "MAIL=mail@example.com",
                "BORN=19700101",
                "FROM=GB",
                "LOC=enGB",
                "SPAM=YN",
                "SINCE=2008.1.1-00:00:00",
                "GFIDS=1",
                "ADDR=" + conn.RemoteIp,
                "TOKEN=pc6r0gHSgZXe1dgwo_CegjBCn24uzUC7KVq1LJDKJ0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.",
            }, prepend: new[]
            {
                System.Text.Encoding.ASCII.GetBytes("_LUID=$0000000000000757"),
            });
            conn.SendResponse("auth", transactionId, authBody);
        }

        private static void HandlePers(FeslConnection conn, uint transactionId,
            IReadOnlyDictionary<string, string> data, FeslPacket packet)
        {
            var player = conn.Player;
            if (player == null)
            {
                conn.SendResponse("pers", transactionId, Array.Empty<string>());
                return;
            }

            // MA= is a raw-bytes line: the game's MADDR prefix (up to '^')
            // plus '^' plus a fixed 40-byte server blob. The 78-byte real
            // capture version overruns the game's 0x40-byte MA parser buffer
            // and the pers response is rejected, so use the 40-byte blob.
            var maddrRaw = packet.RawValues.TryGetValue("MADDR", out var maddr)
                ? maddr : Array.Empty<byte>();
            var prefix = maddrRaw;
            var caret = Array.IndexOf(maddrRaw, (byte)'^');
            if (caret >= 0)
            {
                prefix = maddrRaw[..caret];
            }
            var serverBlob = Convert.FromHexString(
                "c4dcd0a7c9c88bfa94998285a084d28e86c5809082a987c3a2808080808080808080808080808080");
            var maRaw = prefix
                .Concat(new byte[] { (byte)'^' })
                .Concat(serverBlob)
                .ToArray();

            // Build the body with mixed raw/encoded lines in field order:
            // MA= is raw bytes and sits between LA and IDLE.
            var maLine = new byte[3 + maRaw.Length];
            maLine[0] = (byte)'M';
            maLine[1] = (byte)'A';
            maLine[2] = (byte)'=';
            Array.Copy(maRaw, 0, maLine, 3, maRaw.Length);

            var persBody = FeslWriter.BuildBody(new[]
            {
                "NAME=" + player.Gamertag,
                "PERS=" + player.Gamertag,
                "LAST=2018.1.1-00:00:00",
                "PLAST=2018.1.1-00:00:00",
                "SINCE=2008.1.1-00:00:00",
                "PSINCE=2008.1.1-00:00:00",
                "LKEY=000000000000000000000000000.",
                "STAT=,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
                "LOC=enGB",
                "A=" + conn.RemoteIp,
                "LA=" + conn.RemoteIp,
            }, append: new[] { maLine }, trailing: new[]
            {
                "IDLE=50000",
            });
            conn.SendResponse("pers", transactionId, persBody);
        }

        private static void HandleNews(FeslConnection conn, uint transactionId,
            IReadOnlyDictionary<string, string> data)
        {
            var name = FeslPacket.Get(data, "NAME");
            if (name == "client.cfg")
            {
                conn.SendResponse("news", transactionId, StaticData.ClientConfig);
            }
            else
            {
                // Server settings + who update (Python reference uses txn
                // 'ew8' for the settings part).
                conn.SendResponse("news", 0x657738, StaticData.ServerSettings);
                Handlers.Who.SendWho(conn, transactionId, 0);
            }
        }

        private static void HandleSele(FeslConnection conn, uint transactionId,
            IReadOnlyDictionary<string, string> data)
        {
            // Python reference hardcodes these counts; keep identical until
            // the game is proven to accept live values.
            conn.SendResponse("sele", transactionId, new[]
            {
                "GAMES=0",
                "MYGAME=\"1 GAMES=0 ROOMS=0 USERS=1 MESGS=1 MESGTYPES=100728964 STATS=500 RANKS=1 USERSETS=1\"",
                "USERS=0",
                "ROOMS=0",
                "USERSETS=0",
                "MESGS=0",
                "MESGTYPES=0",
                "ASYNC=0",
                "CTRL=0",
                "STATS=0",
                "SLOTS=280",
                "INGAME=0",
                "DP=XBL2/Burnout-Jan2008/mod",
            });
        }

        /// <summary>
        /// Verified fake game state (matches the working Python reference):
        /// a bot host @brobot1023 with COUNT=2 (meets MINSIZE=2), the player
        /// as OPPO1. Bot and player addresses use the TCP peer IP so the
        /// game can actually reach them.
        /// </summary>
        private static List<string> BuildBotGameLines(FeslConnection conn, string sess)
        {
            var player = conn.Player!;
            return new List<string>
            {
                "IDENT=76",
                "WHEN=2024.7.2-8:35:16",
                "NAME=" + player.Gamertag,
                "HOST=@brobot1023",
                "ROOM=0",
                "MAXSIZE=9",
                "MINSIZE=2",
                "COUNT=2",
                "PRIV=0",
                "CUSTFLAGS=413345024",
                "SYSFLAGS=64",
                "EVID=0",
                "EVGID=0",
                "NUMPART=1",
                "SEED=76",
                "GPSHOST=" + player.Gamertag,
                "GPSREGION=0",
                "GAMEMODE=0",
                "GAMEPORT=1000",
                "VOIPPORT=0",
                "WHENC=2024.7.2-8:35:16",
                "SESS=" + sess,
                "PLATPARAMS=" + player.PlatParams,
                "PARTSIZE0=9",
                "PARAMS=" + player.Params,
                "PARTPARAMS0=",
                "OPPO0=@brobot1023",
                "OPPART0=0",
                "OPFLAG0=0",
                "PRES0=0",
                "OPID0=1023",
                "ADDR0=" + conn.RemoteIp,
                "LADDR0=" + conn.RemoteIp,
                "MADDR0=",
                "OPPARAM0=" + player.PlatParams,
                "OPPO1=" + player.Gamertag,
                "OPPART1=0",
                "OPFLAG1=413345024",
                "PRES1=0",
                "OPID1=947",
                "ADDR1=" + conn.RemoteIp,
                "LADDR1=" + conn.RemoteIp,
                "MADDR1=" + player.MacAddress,
                "OPPARAM1=" + player.PlatParams,
            };
        }

        private static void HandleGpsc(FeslConnection conn, uint transactionId,
            IReadOnlyDictionary<string, string> data)
        {
            var player = conn.Player;
            if (player == null)
            {
                return;
            }
            player.PlatParams = FeslPacket.Get(data, "USERPARAMS") ?? "";
            player.Params = FeslPacket.Get(data, "PARAMS") ?? "";

            // Empty ack, then +who (G=76) and the fake bot game via +mgm.
            conn.SendResponse("gpsc", transactionId, Array.Empty<string>());
            Handlers.Who.SendWho(conn, transactionId, 76);
            conn.SendResponse("+mgm", transactionId, BuildBotGameLines(conn, "None"));
        }
        private static void HandleGset(FeslConnection conn, uint transactionId,
            IReadOnlyDictionary<string, string> data)
        {
            var player = conn.Player;
            if (player == null)
            {
                return;
            }

            // The game sends gset multiple times: first with SESS/PLATPARAMS,
            // later with USERPARAMS/USERFLAGS updates (no SESS). Only store
            // SESS when present - the second gset must still get a reply.
            var sess = FeslPacket.Get(data, "SESS");
            if (sess != null)
            {
                player.Session = sess;
            }
            var userParams = FeslPacket.Get(data, "USERPARAMS");
            if (userParams != null)
            {
                player.PlatParams = userParams;
            }
            var paramsv = FeslPacket.Get(data, "PARAMS");
            if (paramsv != null)
            {
                player.Params = paramsv;
            }

            var game = BuildBotGameLines(conn, player.Session);
            conn.SendResponse("gset", transactionId, game);
            // Follow-up +mgm push with the same game state - this is what
            // triggers the game to complete the join.
            conn.SendResponse("+mgm", transactionId, game);
        }
        private static void HandleGdel(FeslConnection conn, uint transactionId)
        {
            var player = conn.Player;
            if (player != null && player.InGame)
            {
                conn.Games.Leave(player.CurrentGameId, player);
            }
            conn.SendResponse("gdel", transactionId, Array.Empty<string>());
            Handlers.Who.SendWho(conn, transactionId, 0);
        }
    }
}
