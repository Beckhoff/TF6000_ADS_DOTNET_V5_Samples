using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwinCAT.Ads;

namespace AdsRouterAndClientConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            AmsNetId remoteSystemId = new AmsNetId("172.17.60.197.1.1");

            Host.CreateDefaultBuilder(args)
                .ConfigureServices((ctx, services) => AddServices(services,remoteSystemId))
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