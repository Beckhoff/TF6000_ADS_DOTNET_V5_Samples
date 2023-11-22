using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads;
using TwinCAT.Ads.Configuration;
using TwinCAT.Ads.SystemService;
using TwinCAT.Ads.TcpRouter;
using TwinCAT.Router;

namespace AdsRouterAndClientConsoleApp
{
    // Very Simple Console Application running Router and Client in one process
    // Demonstrates how the run Router and Client in parallel (asynchronous) without deadlocks.
    internal class SimpleProgram
    {
        // Application settings as Static Variables here
        static AmsNetId _localNetId = new AmsNetId("1.1.1.1.1.1");
        
        static AmsNetId _remoteNetId = new AmsNetId("3.3.3.3.1.1");
        static string _remoteRouteName = "CodedRemote";
        static IPAddress _remoteIp = IPAddress.Parse("192.168.0.3");

        // static ILogger? _logger = null;
        // static ILoggerFactory? _loggerFactory = null;

        private async static Task Main(string[] args)
        {
            try
            {
                using var cancelSource = new CancellationTokenSource();
                var cancel = cancelSource.Token;

                using var routerCancelSource = new CancellationTokenSource();
                var routerCancel = routerCancelSource.Token;

                using var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Information);
                    builder.AddConsole();
                });

                //_logger = loggerFactory.CreateLogger("AdsRouter");

                Console.WriteLine("Starting Router");
                var router = new AmsTcpIpRouter(_localNetId, loggerFactory);

                //Use this overload to instantiate a Router without support of IHost / IConfigurationProvider support and parametrize by code
                //var x = new AmsTcpIpRouter(loggerFactory.CreateLogger("AdsRouter"), _configuration);

                //Apart from using AppSettings configuration, Routes can be added also by code:
                router.AddRoute(new Route(_remoteRouteName, _remoteNetId, new IPAddress[] { _remoteIp }));

                // Starts asynchronously without awaiting!!!
                // So that the router is runnning in a Worker Task in parallel to the Console
                // main thread!
                using Task routerTask = router.StartAsync(routerCancel); // Start the router

                // Starting included AdsServers
                // In this case we add the simple TwinCAT Router (AmsPort 1) to support adding and removing routes
                // (Add-AdsRoute, Remove-AdsRoute from TcXaeMgmt powershell module are supported)
                AdsRouterServer adsRouterService = new AdsRouterServer(router, _logger);

                // And a simple TwinCAT System Service (AmsPort 10000) for supporting browsing routes
                // (Get-AdsRoute support including BroadcastSearch 'Get-AdsRoute -all' in 'TcXaeMgmt' Powershell module)
                SystemServiceServer systemService = new SystemServiceServer(router, _logger);

                Task systemServiceTask = systemService.ConnectServerAndWaitAsync(cancel);
                Task routerServerTask = adsRouterService.ConnectServerAndWaitAsync(cancel);

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
                    ResultReadDeviceState result = await client.ReadStateAsync(cancel);
                    Console.WriteLine($"State: {result.State.AdsState}");

                    Console.WriteLine("Press Enter to read state, 'c' + <Enter> for leave ...");
                    string? line = Console.ReadLine();
                    if (line == "c" || line == "C")
                        stop = true;
                } while (!stop);

                // Cancel AdsServers
                cancelSource.Cancel();

                // Wait for disconnecting all AdsServers
                await Task.WhenAll(systemServiceTask, routerServerTask);

                // Stops the Router
                router.Stop();

                // And wait for the task to finish
                await routerTask;
                Console.WriteLine("Succeeded!");

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
                    //config.AddJsonFile("appSettings.json"); // Use Appsettings Configuration
                    //config.AddEnvironmentVariables("ENV_"); // Use Environment variables
                    //config.AddCommandLine(args); // Use Command Line
                    //config.AddStaticRoutesXmlConfiguration(); // Overriding settings with StaticRoutes.Xml 
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    // Adding console logging here.
                    //logging.AddConsole();
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