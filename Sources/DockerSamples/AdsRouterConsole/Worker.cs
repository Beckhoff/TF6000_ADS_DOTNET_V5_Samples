using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwinCAT.Ads.TcpRouter;
using System.Text;
using TwinCAT.Ads.SystemService;
using TwinCAT.Router;
using TwinCAT.Ads.Configuration;

namespace AdsRouterConsole
{
    /// <summary>
    /// The RouterService instance represents a long running (hosted) service that implements an <see cref="AmsTcpIpRouter"/>.
    /// Implements the <see cref="BackgroundService" />
    /// </summary>
    /// <remarks>
    /// Long running Background task that runs a <see cref="AmsTcpIpRouter."/>.
    /// The service is stopped via the <see cref="CancellationToken"/> given to the <see cref="ExecuteAsync(CancellationToken)"/> method.
    /// </remarks>
    /// <seealso cref="BackgroundService" />
    public class RouterService : BackgroundService
    {
        private readonly ILoggerFactory _loggerFactory;

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
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        public RouterService(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<RouterService>();
            _configuration = configuration;
            //string? value = _configuration.GetValue("ASPNETCORE_ENVIRONMENT", "Production");
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
                // Read the Router Settings from the actual configuration (here set by Environment Variables)
                AmsRouterConfiguration? routerSettings = ConfigurationBinder.Get<AmsRouterConfiguration>(_configuration);

                if (routerSettings?.AmsRouter?.NetId != null)
                {
                    _logger.LogInformation("RouterName  : {Router}", routerSettings.AmsRouter.Name);
                    _logger.LogInformation("LocalNetID  : {NetId}", routerSettings.AmsRouter.NetId);
                    _logger.LogInformation("LoopbackIP  : {IP}", routerSettings.AmsRouter.LoopbackIP);
                    _logger.LogInformation("LoopbackPort: {Port}", routerSettings.AmsRouter.LoopbackPort);

                    //TODO: AmsConfiguration has still to be set before Accessing AmsNetId.Local
                    // if (routerSettings != null)
                    // {
                    //     IPAddress? loopback;
                    //     int loopbackPort = routerSettings.AmsRouter.LoopbackPort;
                    //     bool ok = IPAddress.TryParse(routerSettings.AmsRouter.LoopbackIP, out loopback);

                    //     if (ok)
                    //     {f
                    //         AmsConfiguration.RouterEndPoint = new IPEndPoint(loopback, loopbackPort);
                    //     }
                    // }
                }
                router = new AmsTcpIpRouter(_configuration, _loggerFactory);
                router.RouterStatusChanged += Router_RouterStatusChanged;

                // Use this overload to instantiate a Router without support of IHost/IConfigurationProvider support and parametrize by code
                // AmsTcpIpRouter router = new AmsTcpIpRouter(new AmsNetId("1.2.3.4.5.6"), AmsTcpIpRouter.DEFAULT_TCP_PORT,IPAddress.Loopback,AmsTcpIpRouter.DEFAULT_TCP_PORT,_logger);
                // router.AddRoute(...);
                Console.WriteLine("Press Ctrl + C to shutdown!");
            }

            // Start the router as task
            Task routerTask = router.StartAsync(cancel);

            // Implementation of AdsServer on AmsPort 10000 (SystemService)
            SystemServiceServer systemService = new SystemServiceServer(router,_configuration, _loggerFactory);
            // Implementation of AdsServer on AmsPort 1 (Router)
            AdsRouterServer adsRouterService = new AdsRouterServer(router,_configuration, _loggerFactory);

            // Starting and connecting AdsServers as Task
            Task systemServiceTask = systemService.ConnectServerAndWaitAsync(cancel);
            Task routerServerTask = adsRouterService.ConnectServerAndWaitAsync(cancel);

            // Wait until all Servers have finished.
            await Task.WhenAll(routerTask, systemServiceTask, routerServerTask);
        }

        /// <summary>
        /// Handles the RouterStatusChanged event of the <see cref="AmsTcpIpRouter"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RouterStatusChangedEventArgs"/> instance containing the event data.</param>
        private void Router_RouterStatusChanged(object? sender, RouterStatusChangedEventArgs e)
        {
            if (e.RouterStatus == RouterStatus.Started)
            {
                // From here on, the Router is available to receive Data.
            }
        }
    }
}
