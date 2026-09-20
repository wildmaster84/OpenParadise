namespace OpenParadise.Game
{
    /// <summary>
    /// A connected player. One instance per FESL connection (bound after auth).
    /// </summary>
    public class Player
    {
        public uint Id { get; set; }
        public string Gamertag { get; set; } = "";
        public string Xuid { get; set; } = "";
        public string PersonaId { get; set; } = "";
        public string MacAddress { get; set; } = "";
        public string Address { get; set; } = "";
        public int Port { get; set; }
        public string SessionKey { get; set; } = "";
        public string Friends { get; set; } = "";
        public string PlatParams { get; set; } = "";
        public string Params { get; set; } = "";
        public string Session { get; set; } = "";
        public DateTime ConnectedAt { get; } = DateTime.UtcNow;
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;

        public bool InGame => CurrentGameId != 0;
        public uint CurrentGameId { get; set; }
    }
}
