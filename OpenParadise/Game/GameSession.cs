namespace OpenParadise.Game
{
    /// <summary>
    /// A hosted game (created via gset, discovered via gsea/gpsc).
    /// Mirrors the "+mgm" game-info fields the client understands.
    /// </summary>
    public class GameSession
    {
        public uint Id { get; set; }
        public string HostGamertag { get; set; } = "";
        public uint HostPlayerId { get; set; }
        public string HostAddress { get; set; } = "";
        public int GamePort { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int MaxSize { get; set; } = 8;
        public int MinSize { get; set; } = 2;
        public uint CustFlags { get; set; } = 413345024;
        public uint SysFlags { get; set; } = 64;
        public uint Seed { get; set; }
        public string Params { get; set; } = "";
        public string PlatParams { get; set; } = "";
        public bool Private { get; set; }

        public List<uint> MemberIds { get; } = new();
        public int Count => MemberIds.Count;
        public bool Full => MemberIds.Count >= MaxSize;
        public bool Empty => MemberIds.Count == 0;
    }
}
