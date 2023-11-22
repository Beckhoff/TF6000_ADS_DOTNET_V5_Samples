using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwinCAT.Ads;

namespace AdsOverMqtt
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
    public class AdsOverMqttService : BackgroundService
    {
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<AdsOverMqttService> _logger;
        /// <summary>
        /// Configuration
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="RouterService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        //public AdsOverMqttService(ILogger<AdsOverMqttService> logger, IConfiguration configuration)
        public AdsOverMqttService(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<AdsOverMqttService>();
            _configuration = configuration;

            // The configuration has to be told to the TwinCAT.Ads Framework.
            // Setting the configuration statically so that the local AmsNetId
            // (AmsNetId.Local) is found!
            //GlobalConfiguration.Configuration = _configuration;

            // Use LogLevel.Debug to dump the configuration
            if (_configuration != null)
            {
                foreach (var keyValue in _configuration.AsEnumerable())
                {
                    _logger?.LogDebug($"{keyValue.Key} {keyValue.Value}");
                }
            }
            //string? value = _configuration.GetValue("ASPNETCORE_ENVIRONMENT", "Production");
        }

        /// <summary>
        /// Execute the Router asynchronously as <see cref="BackgroundService"/>.
        /// </summary>
        /// <param name="cancel">The cancellation token.</param>
        protected override async Task ExecuteAsync(CancellationToken cancel)
        {
            // Get the Plc system Target address from configuration
            string? targetAddressStr = _configuration.GetValue("TargetNetId", "");

            if (string.IsNullOrEmpty(targetAddressStr))
                throw new ApplicationException("Configuration for target Address 'TargetNetId' not found!");

            AmsNetId? targetNetId;
            if (!AmsNetId.TryParse(targetAddressStr, out targetNetId))
                throw new ApplicationException($"Configuration 'TargetNetId' doesn't have a valid NetId value (Value: '{targetAddressStr}')");

            try
            {
                AmsAddress targetAddress = new AmsAddress(targetNetId, 851);

                using (var session = new AdsSession(targetAddress, SessionSettings.Default, _configuration, _loggerFactory, this))
                {
                    var connection = (IAdsConnection)session.Connect();

                    while (!cancel.IsCancellationRequested)
                    {
                        // Reading the PLC Symbol MAIN.i every 500 ms
                        string symbol = "MAIN.i";
                        var result = await connection!.ReadValueAsync<short>(symbol, cancel);

                        if (result.Succeeded)
                        {
                            Console.WriteLine($"Address:{connection.Address} Symbol: {symbol} Value: {result.Value}");
                        }
                        else
                        {
                            Console.WriteLine($"AdsError: {result.ErrorCode}");
                        }
                        await Task.Delay(500); // Delay of 500 ms
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.WriteLine("Finished");
        }
    }
}
