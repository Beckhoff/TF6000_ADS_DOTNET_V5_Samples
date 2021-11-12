using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads;
using TwinCAT.Ads.TcpRouter;

namespace AdsRouterAndClientConsoleApp
{
    // Very Simple Console Application running Router and Client in one process
    // Demonstrates how the run Router and Client in parallel (asynchronous) without deadlocks.
    internal class SimpleProgram
    {
        // Application settings as Static Variables here
        static AmsNetId _localNetId = new AmsNetId("1.1.1.1.1.1");
        static AmsNetId _remoteNetId = new AmsNetId("2.2.2.2.1.1");
        static string _remoteRouteName = "Remote1";
        static IPAddress _remoteIp = IPAddress.Parse("192.168.0.2");

        private async static Task Main(string[] args)
        {

            try
            {
                using var cancelTokenSource = new CancellationTokenSource();
                var cancelToken = cancelTokenSource.Token;

                using var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddConsole();
                });

                Console.WriteLine("Starting Router");
                var router = new AmsTcpIpRouter(_localNetId, loggerFactory.CreateLogger("AdsRouter"));
                router.AddRoute(new Route(_remoteRouteName, _remoteNetId, new IPAddress[] { _remoteIp }));

                // Starts asynchronously without awaiting!!!
                // So that the router is runnning in a Worker Task in parallel to the Console
                // main thread!
                using var routerTask = router.StartAsync(cancelToken);

                while (!router.IsRunning)
                {
                    // Wait Asynchronously until Router is running
                    await Task.Delay(1000);
                }

                // Instantiate and connect Client
                using var client = new AdsClient(loggerFactory.CreateLogger("AdsClient"));
                client.Connect(new AmsAddress(_remoteNetId, 10000));


                Console.WriteLine("Client connected");

                bool stop = false;
                do
                {
                    // Read State asynchronously and await
                    ResultReadDeviceState result = await client.ReadStateAsync(cancelToken);

                    Console.WriteLine($"State: {result.State.AdsState}");

                    Console.WriteLine("Press Enter to read state, 'c' + <Enter> for leave ...");
                    string line = Console.ReadLine();
                    if (line == "c" || line == "C")
                        stop = true;
                } while (!stop);

                // Cancel all running tasks
                cancelTokenSource.Cancel();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("Press enter to exit...");
                Console.ReadLine();
            }
        }
    }
    
    // Console application running Router and 2 AdsClients in asynchronous hosted background services
    // and configuring them via AppSettings.
    internal class UseServices
    {
        // AmsNetId of the Remote System
        static AmsNetId _remoteNetId = new AmsNetId("2.2.2.2.1.1");

        private static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((ctx, services) => AddServices(services,_remoteNetId))
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // Add further AppConfigurationProvider here.
                    config.Sources.Clear(); // Clear all default config sources 
                    config.AddJsonFile("appSettings.json"); // Use Appsettings Configuration
                    //config.AddEnvironmentVariables("ENV_"); // Use Environment variables
                    //config.AddCommandLine(args); // Use Command Line
                    //config.AddStaticRoutesXmlConfiguration(); // Overriding settings with StaticRoutes.Xml 
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    // Adding console logging here.
                    logging.AddConsole();
                })
                .Build()
                .Run();
        }   

        private static void AddServices(IServiceCollection services, AmsNetId netId)
        {
            services.AddHostedService<RouterService>();
            services.AddHostedService(ctx =>
                new ClientService1(new AmsAddress(netId, 851), ctx.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(ClientService1))));
            services.AddHostedService(ctx =>
                new ClientService2(new AmsAddress(netId, 851), ctx.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(ClientService2))));
        }
    }
}