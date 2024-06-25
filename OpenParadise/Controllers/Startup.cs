namespace OpenParadise.Controllers
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure services (dependency injection, MVC, etc.)
        }

        public void Configure(IApplicationBuilder app)
        {
            // Configure middleware (logging, routing, etc.)

            // Initialize and start the SocketServer
            var socketServer = new SocketServer(10134); // Replace with your desired port number
            socketServer.Start();

            var socketServer2 = new SocketServer(10135); // Replace with your desired port number
            socketServer2.Start();
        }
    }
}