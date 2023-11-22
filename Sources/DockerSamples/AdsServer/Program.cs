using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwinCAT.Ads.Logging;

namespace Server
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
                Console.WriteLine("Server stopped!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server failed with '{ex.Message}'");
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
                services.AddHostedService<TestServerService>();
            })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                // Add further AppConfigurationProvider here.
                config.Sources.Clear(); // Clear all default config sources 
                config.AddEnvironmentVariables(); // Use Environment variables (Without prefix)
                //config.AddCommandLine(args); // Use Command Lined
                //config.AddJsonFile("appSettings.Development.json"); // Use Appsettings
                //config.AddStaticRoutesXmlConfiguration(); // Overriding settings with StaticRoutes.Xml 
            })
            .ConfigureLogging((context, logging) =>
            {
                var loggerConfig = AdsLoggerConfiguration.CreateFromConfiguration(context.Configuration);
                // var loggerConfig = AdsLoggerConfiguration.CreateFromEnvironment();

                // Overwrites the configured Loglevel programatically
                loggerConfig.LogLevel = LogLevel.Debug;

                // Remove the default logging
                logging.ClearProviders();
                // Adding customized formatted Ads logging here.
                logging.AddProvider(new AdsLoggerProvider(() => loggerConfig));
                logging.SetMinimumLevel(LogLevel.Debug);
                //logging.SetMinimumLevel(LogLevel.Debug);
            });
            return ret;
        }
    }
}
 