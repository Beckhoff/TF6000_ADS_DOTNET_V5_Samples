using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwinCAT.Ads.Logging;

namespace AdsRouterConsole
{
    class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (OperationCanceledException /*cex*/)
            {
                Console.WriteLine("Router cancelledd!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Router failed with '{ex.Message}'");
            }
        }

        /// <summary>
        /// Creates the host builder.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>IHostBuilder.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var ret = Host.CreateDefaultBuilder(args);

            ret.ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<RouterService>();
            })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                // Add further AppConfigurationProvider here.
                config.Sources.Clear(); // Clear all default config sources 
                config.AddEnvironmentVariables(); // Use Environment variables
                //config.AddCommandLine(args); // Use Command Line
                //config.AddJsonFile("appSettings.Development.json"); // Use Appsettings
                //config.AddStaticRoutesXmlConfiguration(); // Overriding settings with StaticRoutes.Xml 
            })
            .ConfigureLogging((context, logging) =>
            {
                var loggerConfig = AdsLoggerConfiguration.CreateFromConfiguration(context.Configuration);
                logging.ClearProviders();
                // Adding console logging here.
                logging.AddProvider(new AdsLoggerProvider(() => loggerConfig));
                logging.SetMinimumLevel(LogLevel.Debug);
            });
            return ret;
        }
    }
}
