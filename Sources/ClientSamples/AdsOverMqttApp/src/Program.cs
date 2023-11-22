using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads.Configuration;
using TwinCAT.Ads.Logging;

namespace AdsOverMqtt
{
    #region CODE_SAMPLE_ADSOVERMQTT

    class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public async static Task Main(string[] args)
        {
            var ret = CreateHostBuilder(args);
            await ret.RunConsoleAsync();
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
                services.AddHostedService<AdsOverMqttService>();
            })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                // Add further AppConfigurationProvider here.
                config.Sources.Clear(); // Clear all default config sources 

                // Different options for configuration
                //config.AddEnvironmentVariables(); // Use Environment variables
                //config.AddCommandLine(args); // Use Command Line
                config.AddJsonFile("appSettings.json"); // Use AppSettings configuration file as config
                //config.AddStaticRoutesXmlConfiguration(null); // Use configuration from StaticRoutes.Xml 
            })
            .ConfigureLogging((context,logging) =>
            {
                // Create specific Ads Logger configuration for formatted output
                AdsLoggerConfiguration? loggerConfig = AdsLoggerConfiguration.CreateFromConfiguration(context.Configuration);
                // Overwrites the configured Loglevel programatically
                // loggerConfig.LogLevel = LogLevel.Debug;

                logging.ClearProviders();
                // Adding console logging here.
                logging.AddProvider(new AdsLoggerProvider(() => loggerConfig));
                logging.SetMinimumLevel(LogLevel.Debug);
            });
            return ret;
        }
    }
    #endregion
}
