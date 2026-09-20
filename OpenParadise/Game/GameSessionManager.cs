namespace OpenParadise.Game
{
    /// <summary>
    /// Global game-session registry (hosted games). Thread-safe.
    /// </summary>
    public class GameSessionManager
    {
        private readonly object _lock = new();
        private readonly Dictionary<uint, GameSession> _games = new();
        private uint _nextId = 73;

        public GameSession Create(Player host, int maxSize, int minSize, string gamePort)
        {
            lock (_lock)
            {
                var game = new GameSession
                {
                    Id = _nextId++,
                    HostGamertag = host.Gamertag,
                    HostPlayerId = host.Id,
                    HostAddress = host.Address,
                    GamePort = int.TryParse(gamePort, out var p) ? p : 3074,
                    MaxSize = maxSize,
                    MinSize = minSize,
                    Seed = _nextId - 1,
                };
                game.MemberIds.Add(host.Id);
                _games[game.Id] = game;
                host.CurrentGameId = game.Id;
                return game;
            }
        }

        public GameSession? GetById(uint id)
        {
            lock (_lock)
            {
                return _games.TryGetValue(id, out var g) ? g : null;
            }
        }

        public void Remove(uint id)
        {
            lock (_lock)
            {
                _games.Remove(id);
            }
        }

        public void Leave(uint gameId, Player player)
        {
            lock (_lock)
            {
                if (_games.TryGetValue(gameId, out var game))
                {
                    game.MemberIds.Remove(player.Id);
                    if (game.MemberIds.Count == 0)
                    {
                        _games.Remove(gameId);
                    }
                }
                if (player.CurrentGameId == gameId)
                {
                    player.CurrentGameId = 0;
                }
            }
        }

        public List<GameSession> All()
        {
            lock (_lock)
            {
                return _games.Values.ToList();
            }
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _games.Count;
                }
            }
        }
    }
}
