using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using OpenParadise.Fesl;
using OpenParadise.Game;

namespace OpenParadise.Net
{
    /// <summary>
    /// One connected client: owns its socket, receive framing, multipart
    /// reassembly, heartbeat, and per-connection player state.
    /// </summary>
    public class FeslConnection
    {
        private readonly Socket _socket;
        private readonly byte[] _recvBuffer = new byte[8192];
        private readonly MemoryStream _pending = new();
        private readonly ConcurrentDictionary<uint, byte[]> _multipart = new();
        private readonly CancellationTokenSource _cts = new();

        public EndPoint? Remote { get; }
        public string RemoteIp { get; }
        public Player? Player { get; set; }
        public PlayerManager Players { get; }
        public GameSessionManager Games { get; }

        public event Action<FeslConnection>? Disconnected;

        public FeslConnection(Socket socket, PlayerManager players, GameSessionManager games)
        {
            _socket = socket;
            Remote = socket.RemoteEndPoint;
            RemoteIp = (Remote as IPEndPoint)?.Address.ToString() ?? "0.0.0.0";
            Players = players;
            Games = games;
        }

        public async Task RunAsync()
        {
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    int bytesRead;
                    try
                    {
                        bytesRead = await _socket.ReceiveAsync(_recvBuffer, SocketFlags.None);
                    }
                    catch (Exception)
                    {
                        break; // disconnect / reset
                    }

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    lock (_pending)
                    {
                        _pending.Write(_recvBuffer, 0, bytesRead);
                        ProcessPending();
                    }
                }
            }
            finally
            {
                OnDisconnect();
            }
        }

        private void ProcessPending()
        {
            var data = _pending.ToArray();
            var span = data.AsSpan();
            int offset = 0;

            while (offset < span.Length)
            {
                FeslPacket? packet;
                int consumed;
                try
                {
                    packet = FeslPacket.TryParse(span[offset..], out consumed);
                }
                catch (InvalidDataException ex)
                {
                    Log($"[FESL] stream corruption: {ex.Message} - dropping connection");
                    Close();
                    return;
                }

                if (packet == null)
                {
                    break; // incomplete packet, wait for more data
                }

                offset += consumed;

                if (packet.Type == FeslPacketType.MultipartRequest)
                {
                    // Accumulate; the final part arrives as a single request
                    // with the same TXN ID.
                    _multipart.AddOrUpdate(packet.TransactionId,
                        _ => packet.Body,
                        (_, existing) => existing.Concat(packet.Body).ToArray());
                    continue;
                }

                byte[] fullBody;
                if (_multipart.TryRemove(packet.TransactionId, out var prefix))
                {
                    fullBody = prefix.Concat(packet.Body).ToArray();
                }
                else
                {
                    fullBody = packet.Body;
                }

                _ = FeslServer.DispatchAsync(this, packet,
                    FeslPacket.ParseBody(fullBody));
            }

            // Keep only the unconsumed remainder.
            var remainder = data[offset..];
            _pending.SetLength(0);
            if (remainder.Length > 0)
            {
                _pending.Write(remainder, 0, remainder.Length);
            }
        }

        public void StartHeartbeat()
        {
            if (_heartbeatStarted)
            {
                return;
            }
            _heartbeatStarted = true;
            _ = HeartbeatLoopAsync();
        }

        private bool _heartbeatStarted;

        private async Task HeartbeatLoopAsync()
        {
            try
            {
                // Send the first ping immediately (the working server sends
                // one as soon as the client registers its address).
                var now = DateTime.Now;
                var body = FeslWriter.BuildBody(new[] { $"REF={now:yyyy.M.d-HH:mm:ss}" });
                Send(FeslWriter.Build("~png", FeslPacketType.Ping, 0, body));

                while (!_cts.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(15), _cts.Token);
                    now = DateTime.Now;
                    body = FeslWriter.BuildBody(new[] { $"REF={now:yyyy.M.d-HH:mm:ss}" });
                    Send(FeslWriter.Build("~png", FeslPacketType.Ping, 0, body));
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception)
            {
                Close();
            }
        }

        /// <summary>Sends a raw packet (thread-safe).</summary>
        public void Send(byte[] packet)
        {
            try
            {
                lock (_socket)
                {
                    _socket.Send(packet);
                }
            }
            catch (Exception)
            {
                Close();
            }
        }

        /// <summary>Sends a response with the given body lines.</summary>
        public void Send(string command, FeslPacketType type, uint transactionId,
            IEnumerable<string> lines)
        {
            var body = FeslWriter.BuildBody(lines);
            Send(FeslWriter.Build(command, type, transactionId, body));
        }

        /// <summary>Sends a response with the given body lines.</summary>
        public void SendResponse(string command, uint transactionId, IEnumerable<string> lines)
        {
            SendResponse(command, transactionId, FeslWriter.BuildBody(lines));
        }

        /// <summary>
        /// Sends a response with a pre-built body. Type selection mirrors the
        /// verified Python reference: PING (0x00) for the known command set
        /// (unless the txn is 'ew8'/'fnd'), 0x6E for news-ew8/gqwk, and
        /// single/multipart response by body size otherwise.
        /// </summary>
        public void SendResponse(string command, uint transactionId, byte[] body)
        {
            // 'ew8' and 'fnd' are 3-byte ASCII txn ids carried in the txn
            // field; compare the low 3 bytes.
            var txnLow = transactionId & 0xFFFFFF;
            var isEw8 = txnLow == 0x657738;   // "ew8"
            var isFnd = txnLow == 0x666E64;   // "fnd"

            FeslPacketType type;
            if (command is "@dir" or "skey" or "sele" or "auth" or "pers" or "fget" or
                "+who" or "+mgm" or "news" or "usld" or "gsea" or "snap" or "opup" or
                "gdel" or "fbst" or "rrup" or "slst" or "gpsc" or "fupd" or "sviw" or
                "sdta" or "gset" or "hchk" or "cate")
            {
                type = isEw8 || isFnd ? FeslPacketType.Unknown6E : FeslPacketType.Ping;
            }
            else if (command == "gqwk")
            {
                type = FeslPacketType.Unknown6E;
            }
            else
            {
                type = body.Length <= 1024
                    ? FeslPacketType.SingleResponse
                    : FeslPacketType.MultipartResponse;
            }

            Send(FeslWriter.Build(command, type, transactionId, body));
        }

        public void Close()
        {
            try
            {
                _cts.Cancel();
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
            }
            _socket.Close();
        }

        private void OnDisconnect()
        {
            _cts.Cancel();
            if (Player != null)
            {
                if (Player.CurrentGameId != 0)
                {
                    Games.Leave(Player.CurrentGameId, Player);
                }
                Players.Remove(Player);
                Log($"{Player.Gamertag} disconnected");
            }
            Disconnected?.Invoke(this);
        }

        public void Log(string message) =>
            Console.WriteLine($"[{RemoteIp}] {message}");
    }
}
