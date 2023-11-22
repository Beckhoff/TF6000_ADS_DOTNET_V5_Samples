using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads;
using TwinCAT.Ads.TcpRouter;

namespace AdsRouterAndClientConsoleApp
{
    internal class RouterService : BackgroundService
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<RouterService> _logger;
        private readonly IConfiguration _configuration;

        public RouterService(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<RouterService>();
            _configuration = configuration;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var router = new AmsTcpIpRouter(_configuration,_loggerFactor);
            //_logger.LogInformation("Information!");
            //_logger.LogDebug("Debug!");
            //_logger.LogTrace("Trace!");
            //_logger.LogError("Error!");

            return router.StartAsync(stoppingToken);
        }
    }
}