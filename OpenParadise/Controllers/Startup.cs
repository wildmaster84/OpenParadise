using OpenParadise.Game;
using OpenParadise.Net;

namespace OpenParadise.Controllers
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app)
        {
            Config.Load(_configuration);
            FeslServer.Debug = Config.Debug;

            // Main FESL port (the port the game connects to first; @dir
            // redirects to ServerPort).
            var fesl = new FeslServer(Config.ServerPort);
            fesl.Start();

            Console.WriteLine($"OpenParadise(v2.0.0): FESL server on port {Config.ServerPort} " +
                $"(advertised IP {Config.ServerIp})");
        }
    }
}
