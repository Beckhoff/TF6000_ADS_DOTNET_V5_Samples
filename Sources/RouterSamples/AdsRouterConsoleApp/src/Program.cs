﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using TwinCAT.Ads.Logging;
using TwinCAT.Ads.TcpRouter;

namespace TwinCAT.Ads.AdsRouterService
{
    #region CODE_SAMPLE_ROUTERCONSOLE

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
                Console.WriteLine("Router cancelled!");
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
                //config.Sources.Clear(); // Clear all default config sources 
                config.AddEnvironmentVariables("ENV_"); // Use Environment variables
                //config.AddCommandLine(args); // Use Command Line
                //config.AddJsonFile("appSettings.json"); // Use Appsettings
                //config.AddStaticRoutesXmlConfiguration(); // Overriding settings with StaticRoutes.Xml 
            })
            .ConfigureLogging((context, logging) =>
            {
                 //var loggerConfig = AdsLoggerConfiguration.CreateFromEnvironment();
                 var loggerConfig = AdsLoggerConfiguration.CreateFromConfiguration(context.Configuration);

                 logging.SetMinimumLevel(LogLevel.Debug);
                 logging.ClearProviders();
                 
                 // Using simplified logging instead of standard console logging.
                 logging.AddProvider(new AdsLoggerProvider(() => loggerConfig));
                 // Adding Standard console logging here.
                 //logging.AddConsole();
            });
            return ret;
        }
    }
    #endregion
}
