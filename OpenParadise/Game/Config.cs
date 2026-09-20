namespace OpenParadise.Game
{
    /// <summary>
    /// Server configuration. Loaded from appsettings.json with env-var
    /// overrides; falls back to sensible defaults.
    /// </summary>
    public static class Config
    {
        public static string ServerIp { get; set; } = "127.0.0.1";
        public static int ServerPort { get; set; } = 10134;
        public static int BuddyPort { get; set; } = 13505;
        public static int QosPort { get; set; } = 17582;
        public static bool Debug { get; set; } = true;

        public static void Load(IConfiguration configuration)
        {
            ServerIp = configuration["Fesl:ServerIp"] ?? ServerIp;
            _ = int.TryParse(configuration["Fesl:ServerPort"], out var port);
            if (port > 0) ServerPort = port;
            _ = int.TryParse(configuration["Fesl:BuddyPort"], out var buddy);
            if (buddy > 0) BuddyPort = buddy;
            _ = int.TryParse(configuration["Fesl:QosPort"], out var qos);
            if (qos > 0) QosPort = qos;
            _ = bool.TryParse(configuration["Fesl:Debug"], out var dbg);
            Debug = configuration["Fesl:Debug"] != null ? dbg : Debug;
        }
    }
}
