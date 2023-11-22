using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwinCAT.Ads.Logging;
using TwinCAT.Ads.Mqtt;

namespace Client
{
    class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            //TODO: Remove
            //HACK: Forces to publish the AdsOverMqtt package to the docker container!!!
            MqttAmsServerNetFactory x = new MqttAmsServerNetFactory();

            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (OperationCanceledException /*cex*/)
            {
                Console.WriteLine("AdsClient cancelled!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdsClient failed with '{ex.Message}'");
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
                // Adds the Service to the Host
                services.AddHostedService<ClientService>();
            })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                // Add further AppConfigurationProvider here.
                config.Sources.Clear(); // Clear all default config sources 
                config.AddEnvironmentVariables(); // Use Environment variables (Without prefix)
                //config.AddCommandLine(args); // Use Command Line
                //config.AddJsonFile("appSettings.Development.json"); // Use Appsettings
                //config.AddStaticRoutesXmlConfiguration(); // Overriding settings with StaticRoutes.Xml 
            })
            .ConfigureLogging((context, logging) =>
            {
                // Create a Logger Configuration from Configration context
                var loggerConfig = AdsLoggerConfiguration.CreateFromConfiguration(context.Configuration);
                // var loggerConfig = AdsLoggerConfiguration.CreateFromEnvironment();
                // Overwrites the configured Loglevel programatically
                loggerConfig.LogLevel = LogLevel.Debug;

                // Remove the default logging
                logging.ClearProviders();
                // Adding customized formatted Ads logging here.
                logging.AddProvider(new AdsLoggerProvider(() => loggerConfig));
                // logging.SetMinimumLevel(LogLevel.Information);
                logging.SetMinimumLevel(LogLevel.Debug);
            });
            return ret;
        }
    }
}
