namespace OpenParadise.Fesl
{
    /// <summary>
    /// Builds FESL/Plasma packets for sending.
    /// </summary>
    public static class FeslWriter
    {
        /// <summary>
        /// Builds a packet from raw body bytes. The body should already be
        /// terminated with a NUL byte if the client expects one.
        /// </summary>
        public static byte[] Build(string command, FeslPacketType type, uint transactionId,
            byte[] body)
        {
            var size = FeslPacket.HeaderSize + body.Length;
            var packet = new byte[size];

            var cmd = System.Text.Encoding.ASCII.GetBytes(command.PadRight(4, '\0'));
            System.Array.Copy(cmd, 0, packet, 0, 4);
            packet[4] = (byte)type;
            packet[5] = (byte)(transactionId >> 16);
            packet[6] = (byte)(transactionId >> 8);
            packet[7] = (byte)transactionId;
            packet[8] = (byte)(size >> 24);
            packet[9] = (byte)(size >> 16);
            packet[10] = (byte)(size >> 8);
            packet[11] = (byte)size;
            System.Array.Copy(body, 0, packet, FeslPacket.HeaderSize, body.Length);

            return packet;
        }

        /// <summary>
        /// Builds a packet body from key=value pairs, separated by \t and
        /// terminated with a NUL byte.
        /// </summary>
        public static byte[] BuildBody(IEnumerable<string> lines,
            IEnumerable<byte[]>? prepend = null,
            IEnumerable<byte[]>? append = null,
            IEnumerable<string>? trailing = null)
        {
            using var ms = new System.IO.MemoryStream();
            bool first = true;

            void Sep()
            {
                if (!first)
                {
                    ms.WriteByte((byte)'\t');
                }
                first = false;
            }

            if (prepend != null)
            {
                foreach (var raw in prepend)
                {
                    Sep();
                    ms.Write(raw, 0, raw.Length);
                }
            }
            foreach (var line in lines)
            {
                Sep();
                WriteEncodedLine(ms, line);
            }
            if (append != null)
            {
                foreach (var raw in append)
                {
                    Sep();
                    ms.Write(raw, 0, raw.Length);
                }
            }
            if (trailing != null)
            {
                foreach (var line in trailing)
                {
                    Sep();
                    WriteEncodedLine(ms, line);
                }
            }
            ms.WriteByte(0);
            return ms.ToArray();
        }

        /// <summary>
        /// Writes one key=value line, decoding "$hex" and "Â§hex" values back
        /// to raw bytes (matching the Python reference server).
        /// </summary>
        private static void WriteEncodedLine(System.IO.MemoryStream ms, string line)
        {
            var eq = line.IndexOf('=');
            if (eq < 0)
            {
                var plain = System.Text.Encoding.ASCII.GetBytes(line);
                ms.Write(plain, 0, plain.Length);
                return;
            }

            var key = line[..eq];
            var value = line[(eq + 1)..];

            ms.Write(System.Text.Encoding.ASCII.GetBytes(key), 0, key.Length);
            ms.WriteByte((byte)'=');

            if (value.StartsWith('$') && IsHexString(value[1..]))
            {
                // "$hex" -> keep the $ marker, then decoded bytes (mirrors
                // the Python reference's parse/send round-trip).
                ms.WriteByte((byte)'$');
                WriteHex(ms, value[1..]);
            }
            else if (value.StartsWith('\u00A7') && IsHexString(value[1..]))
            {
                // "Â§hex" -> raw bytes, no marker.
                WriteHex(ms, value[1..]);
            }
            else
            {
                var bytes = System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(value);
                ms.Write(bytes, 0, bytes.Length);
            }
        }

        private static bool IsHexString(string s)
        {
            if (s.Length == 0 || s.Length % 2 != 0)
            {
                return false;
            }
            foreach (var c in s)
            {
                if (!Uri.IsHexDigit(c))
                {
                    return false;
                }
            }
            return true;
        }

        private static void WriteHex(System.IO.MemoryStream ms, string hex)
        {
            for (int i = 0; i + 1 < hex.Length; i += 2)
            {
                ms.WriteByte(Convert.ToByte(hex.Substring(i, 2), 16));
            }
        }
    }
}
