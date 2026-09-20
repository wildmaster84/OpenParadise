namespace OpenParadise.Fesl
{
    /// <summary>
    /// FESL/Plasma packet types (byte 4 of the header).
    /// </summary>
    public enum FeslPacketType : byte
    {
        /// <summary>Ping/heartbeat (also used as response type for several commands).</summary>
        Ping = 0x00,

        /// <summary>Single-packet response (server -> client).</summary>
        SingleResponse = 0x80,

        /// <summary>Multipart response (server -> client, large bodies).</summary>
        MultipartResponse = 0xB0,

        /// <summary>Single-packet request (client -> server).</summary>
        SingleRequest = 0xC0,

        /// <summary>Multipart request (client -> server, large bodies).</summary>
        MultipartRequest = 0xF0,

        /// <summary>Unknown type observed for "news" (txn ew8) and "gqwk" responses.</summary>
        Unknown6E = 0x6E,
    }
}
