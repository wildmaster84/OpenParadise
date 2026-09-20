namespace OpenParadise.Fesl
{
    /// <summary>
    /// A parsed FESL/Plasma packet.
    /// Header (12 bytes): 4-byte command ("acct", "skey", ...), 1-byte packet
    /// type, 3-byte transaction ID, 4-byte big-endian total size (header+body).
    /// Body: key=value pairs separated by \t (0x09) or \n (0x0A), terminated by
    /// a NUL byte.
    /// </summary>
    public class FeslPacket
    {
        public const int HeaderSize = 12;

        public string Command { get; }
        public FeslPacketType Type { get; }
        public uint TransactionId { get; }
        public uint Size { get; }
        public byte[] Body { get; }

        /// <summary>Parsed key=value map from the body (empty for ping bodies).</summary>
        public IReadOnlyDictionary<string, string> Values { get; }

        /// <summary>
        /// Raw wire bytes of each value (including any '$' prefix byte).
        /// The Python reference keeps these for binary passthrough (pers MA).
        /// </summary>
        public IReadOnlyDictionary<string, byte[]> RawValues { get; }

        public FeslPacket(string command, FeslPacketType type, uint transactionId,
            byte[] body, IReadOnlyDictionary<string, string> values,
            IReadOnlyDictionary<string, byte[]>? rawValues = null)
        {
            Command = command;
            Type = type;
            TransactionId = transactionId;
            Size = HeaderSize + (uint)body.Length;
            Body = body;
            Values = values;
            RawValues = rawValues ?? new Dictionary<string, byte[]>();
        }

        public string? GetValue(string key) =>
            Values.TryGetValue(key, out var v) ? v : null;

        /// <summary>Convenience lookup on a parsed-values dictionary.</summary>
        public static string? Get(IReadOnlyDictionary<string, string> data, string key) =>
            data.TryGetValue(key, out var v) ? v : null;

        /// <summary>
        /// Attempts to parse a full packet from the start of the buffer.
        /// Returns null if the buffer doesn't contain a complete packet yet.
        /// </summary>
        public static FeslPacket? TryParse(ReadOnlySpan<byte> buffer, out int consumed)
        {
            consumed = 0;
            if (buffer.Length < HeaderSize)
            {
                return null;
            }

            var command = System.Text.Encoding.ASCII.GetString(buffer[..4]).TrimEnd('\0');
            var type = (FeslPacketType)buffer[4];
            var transactionId = ((uint)buffer[5] << 16) | ((uint)buffer[6] << 8) | buffer[7];
            var size = ((uint)buffer[8] << 24) | ((uint)buffer[9] << 16) |
                       ((uint)buffer[10] << 8) | buffer[11];

            // Sanity check: size must cover the header and not be absurd.
            if (size < HeaderSize || size > 10 * 1024 * 1024)
            {
                // Corrupt stream - let the caller decide how to handle it.
                throw new InvalidDataException(
                    $"FESL packet with invalid size {size} (command '{command}')");
            }

            if (buffer.Length < size)
            {
                return null; // need more data
            }

            consumed = (int)size;
            var body = buffer.Slice(HeaderSize, (int)size - HeaderSize).ToArray();
            var (values, raw) = ParseBodyRaw(body);

            return new FeslPacket(command, type, transactionId, body, values, raw);
        }

        /// <summary>
        /// Parses the body into key=value pairs. Lines are separated by \n
        /// ONLY (the Python reference splits on \n; splitting on \t too
        /// corrupts binary values that contain 0x09). Values may be binary;
        /// binary values are hex-encoded with a '$' or '§' prefix in the
        /// string view, and the raw bytes are kept in rawResult.
        /// </summary>
        public static (Dictionary<string, string> values,
            Dictionary<string, byte[]> raw) ParseBodyRaw(byte[] body)
        {
            var result = new Dictionary<string, string>();
            var rawResult = new Dictionary<string, byte[]>();
            if (body.Length == 0)
            {
                return (result, rawResult);
            }

            // Strip trailing NUL.
            var end = body.Length;
            while (end > 0 && body[end - 1] == 0)
            {
                end--;
            }

            var span = body.AsSpan(0, end);
            int pos = 0;
            while (pos < span.Length)
            {
                // Find end of line (\n only).
                int lineEnd = pos;
                while (lineEnd < span.Length && span[lineEnd] != (byte)'\n')
                {
                    lineEnd++;
                }

                var line = span.Slice(pos, lineEnd - pos);
                pos = lineEnd + 1;

                var eq = line.IndexOf((byte)'=');
                if (eq <= 0)
                {
                    continue;
                }

                var key = System.Text.Encoding.ASCII.GetString(line[..eq]);
                var value = line[(eq + 1)..];

                rawResult[key] = value.ToArray();
                result[key] = EncodeValue(value);
            }

            return (result, rawResult);
        }

        /// <summary>Back-compat wrapper.</summary>
        public static Dictionary<string, string> ParseBody(byte[] body) =>
            ParseBodyRaw(body).values;

        private static string EncodeValue(ReadOnlySpan<byte> value)
        {
            // "$"-prefixed values round-trip as literal ASCII, matching the
            // Python reference: parse stores "$" + hex(bytes-after-$), and the
            // writer decodes that back to the original bytes. Net effect on
            // the wire: identity.
            if (value.Length > 0 && value[0] == (byte)'$')
            {
                return "$" + Convert.ToHexString(value[1..]);
            }

            // If the value is pure printable ASCII, keep it as-is.
            bool printable = true;
            foreach (var b in value)
            {
                if (b < 0x20 || b > 0x7E)
                {
                    printable = false;
                    break;
                }
            }

            if (printable)
            {
                return System.Text.Encoding.ASCII.GetString(value);
            }

            // Binary value: hex-encode with Â§ prefix (wire convention).
            var sb = new System.Text.StringBuilder("\u00A7");
            foreach (var b in value)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
