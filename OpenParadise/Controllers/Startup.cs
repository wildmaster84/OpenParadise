namespace OpenParadise.Controllers
{
    public class Startup
    {
        public static int ServerPort = 10135;
        public static int ProxyPort = 10134;
        public static String ServerIP = "68.46.244.148";
        public static String version = "v1.0.0";
        public static bool debug = true;
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
            Console.WriteLine($"OpenParadise({version}): Running on ports {ProxyPort} and {ServerPort}");
        }
    }
}