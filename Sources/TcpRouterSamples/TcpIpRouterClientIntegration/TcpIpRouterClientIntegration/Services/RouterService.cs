using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads;
using TwinCAT.Ads.TcpRouter;

namespace TcpIpRouterClientIntegration.Services
{
    internal class RouterService : BackgroundService
    {
        private readonly ILogger<RouterService> _logger;
        private readonly IConfiguration _configuration;

        public RouterService(ILogger<RouterService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var router = new AmsTcpIpRouter(_logger, _configuration);
            //_logger.LogInformation("Information!");
            //_logger.LogDebug("Debug!");
            //_logger.LogTrace("Trace!");
            //_logger.LogError("Error!");

            return router.StartAsync(stoppingToken);
        }
    }
}