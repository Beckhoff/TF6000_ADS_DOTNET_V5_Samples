using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using TwinCAT.Ads;
using TwinCAT.Ads.Configuration;
using TwinCAT.Ams;

namespace Server
{
    /// <summary>
    /// The TestServerService instance represents a long running (hosted) service that implements an <see cref="AdsServer"/>.
    /// Implements the <see cref="BackgroundService" />
    /// </summary>
    /// <remarks>
    /// Long running Background task that runs a <see cref="AmsTcpIpRouter."/>.
    /// The service is stopped via the <see cref="CancellationToken"/> given to the <see cref="ExecuteAsync(CancellationToken)"/> method.
    /// </remarks>
    /// <seealso cref="BackgroundService" />
    public class TestServerService : BackgroundService
    {
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<TestServerService> _logger;
        /// <summary>
        /// Configuration
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestServerService"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        public TestServerService(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<TestServerService>();
            _configuration = configuration;

            // The configuration has to be told to the TwinCAT.Ads Framework
            // Setting the configuration statically so that the local AmsNetId
            // (AmsNetId.Local) is found!
            //GlobalConfiguration.Configuration = _configuration;

            // string? value = _configuration.GetValue("ASPNETCORE_ENVIRONMENT", "Production");

            // // Dumps the configuration
            // foreach(var keyValue in _configuration.AsEnumerable())
            // {
            //     _logger.LogDebug($"{keyValue.Key} {keyValue.Value}");
            // }
        }

        /// <summary>
        /// Execute the Router asynchronously as <see cref="BackgroundService"/>.
        /// </summary>
        /// <param name="cancel">The cancellation token.</param>
        protected override async Task ExecuteAsync(CancellationToken cancel)
        {
            SymbolicTestServer server;
            //Console.WriteLine("Press Ctrl + C to shutdown!");

            // AmsNetId? local = null;
            // AmsNetId.TryGetLocalNetId(_logger,out local);
            // _logger.LogInformation("X '{Local}'",local);

            // This client will be connected to a logically local (other docker container) AdsServer runnning on port 25000
            AmsAddress address = new AmsAddress(AmsNetId.Local, 25000);
            _logger.LogInformation("AdsServer starting on '{Address}'",address);

            server = new SymbolicTestServer(_configuration, _loggerFactory);
            server.ServerConnectionStateChanged += Server_ServerConnectionStateChanged;
 
            // Wait until the Server Disconnects/Disposes itself (by cancel token)
            AdsErrorCode errorCode = await server.ConnectServerAndWaitAsync(cancel);

            if (errorCode.Succeeded())
            {
                _logger?.LogInformation("Server '{Server}' finished",server);
            }
            else
            {
                _logger?.LogError("Server '{Server}' finished with error '{Error}'", server, errorCode);
            }
        }

        private void Server_ServerConnectionStateChanged(object? sender, TwinCAT.Ads.Server.ServerConnectionStateChangedEventArgs e)
        {
            _logger?.LogInformation("'{Name}' State:{State}",(SymbolicTestServer)sender!,e.State);
        }
    }
}
