using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwinCAT.Ads.TcpRouter;
using System.Net;
using System.Text;

namespace TwinCAT.Ads.AdsRouterService
{

    #region CODE_SAMPLE_ROUTERCONSOLE

    /// <summary>
    /// The RouterService instance represents a long running (hosted) service that implements an <see cref="AmsTcpIpRouter"/>.
    /// Implements the <see cref="Microsoft.Extensions.Hosting.BackgroundService" />
    /// </summary>
    /// <remarks>
    /// Long running Background task that runs a <see cref="AmsTcpIpRouter."/>.
    /// The service is stopped via the <see cref="CancellationToken"/> given to the <see cref="RouterService.ExecuteAsync(CancellationToken)"/> method.
    /// </remarks>
    /// <seealso cref="Microsoft.Extensions.Hosting.BackgroundService" />
    public class RouterService : BackgroundService
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<RouterService> _logger;
        /// <summary>
        /// Configuration
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="RouterService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        public RouterService(ILogger<RouterService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            //string value = (string)_configuration.GetValue("ASPNETCORE_ENVIRONMENT", "Production");
        }

        /// <summary>
        /// Execute the Router asynchronously as <see cref="BackgroundService"/>.
        /// </summary>
        /// <param name="cancel">The cancellation token.</param>
        protected override async Task ExecuteAsync(CancellationToken cancel)
        {
            AmsTcpIpRouter router;

            using (_logger.BeginScope("Starting"))
            {
                StringBuilder appCommon = new StringBuilder();

                appCommon.AppendLine($"ApplicationPath: {Environment.GetCommandLineArgs()[0]}");
                appCommon.AppendLine($"BaseDirectory: {System.AppContext.BaseDirectory}");
                appCommon.AppendLine($"CurrentDirectory: {Directory.GetCurrentDirectory()}");
                //_logger.LogInformation(sB.ToString());

                StringBuilder config = new StringBuilder();
                string value = (string)_configuration.GetValue("ASPNETCORE_ENVIRONMENT", "Production");
                config.AppendLine($"ASPNETCORE_ENVIRONMENT: {value}");

                Console.WriteLine("Application Directories");
                Console.WriteLine("=======================");
                Console.WriteLine(appCommon);
                Console.WriteLine("");
                Console.WriteLine("Configuration");
                Console.WriteLine("=============");
                Console.WriteLine(config);
                Console.WriteLine("");

                Console.WriteLine("Press Ctrl + C to shutdown!");

                router = new AmsTcpIpRouter(_logger, _configuration);
                router.RouterStatusChanged += Router_RouterStatusChanged;

                // Use this overload to instantiate a Router without support of IHost/IConfigurationProvider support and parametrize by code
                // AmsTcpIpRouter router = new AmsTcpIpRouter(new AmsNetId("1.2.3.4.5.6"), AmsTcpIpRouter.DEFAULT_TCP_PORT,IPAddress.Loopback,AmsTcpIpRouter.DEFAULT_TCP_PORT,_logger);
                // router.AddRoute(...);

                _logger.LogInformation(appCommon.ToString());
                _logger.LogInformation(config.ToString());
            }

            Task routerTask = router.StartAsync(cancel); // Start the router
            await routerTask;
        }

        /// <summary>
        /// Handles the RouterStatusChanged event of the <see cref="AmsTcpIpRouter"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RouterStatusChangedEventArgs"/> instance containing the event data.</param>
        private void Router_RouterStatusChanged(object sender, RouterStatusChangedEventArgs e)
        {
            if (e.RouterStatus == RouterStatus.Started)
            {
                // From here on, the Router is available to receive Data.
            }
        }
    }
#endregion
}
