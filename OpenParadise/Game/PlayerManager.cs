namespace OpenParadise.Game
{
    /// <summary>
    /// Global player registry. Thread-safe.
    /// </summary>
    public class PlayerManager
    {
        private readonly object _lock = new();
        private readonly Dictionary<uint, Player> _byId = new();
        private readonly Dictionary<string, Player> _byGamertag = new(StringComparer.OrdinalIgnoreCase);
        private uint _nextId = 1022;

        public Player Create(string gamertag)
        {
            lock (_lock)
            {
                var player = new Player
                {
                    Id = _nextId++,
                    Gamertag = gamertag,
                };
                _byId[player.Id] = player;
                _byGamertag[gamertag] = player;
                return player;
            }
        }

        public Player? GetById(uint id)
        {
            lock (_lock)
            {
                return _byId.TryGetValue(id, out var p) ? p : null;
            }
        }

        public Player? GetByGamertag(string gamertag)
        {
            lock (_lock)
            {
                return _byGamertag.TryGetValue(gamertag, out var p) ? p : null;
            }
        }

        public void Remove(Player player)
        {
            lock (_lock)
            {
                _byId.Remove(player.Id);
                _byGamertag.Remove(player.Gamertag);
            }
        }

        public List<Player> All()
        {
            lock (_lock)
            {
                return _byId.Values.ToList();
            }
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _byId.Count;
                }
            }
        }
    }
}
