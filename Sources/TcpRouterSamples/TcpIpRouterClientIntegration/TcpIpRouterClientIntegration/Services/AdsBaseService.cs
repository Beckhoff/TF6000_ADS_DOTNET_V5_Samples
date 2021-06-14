using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads;

namespace TcpIpRouterClientIntegration
{
    internal abstract class AdsBaseService : BackgroundService
    {
        protected readonly ILogger _logger;
        protected readonly AmsAddress _address;

        protected AdsClient _client = null;

        public AdsBaseService(AmsAddress address, ILogger logger)
        {
            _address = address;
            _logger = logger;
        }

        ~AdsBaseService()
        {
            Dispose(false);
        }

        bool _disposed = false;

        public override void Dispose()
        {
            if (!_disposed)
                Dispose(true);

            base.Dispose();

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_client != null)
                    _client.Dispose();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken cancel)
        {
            // Wait for router to start
            await Task.Delay(TimeSpan.FromSeconds(2), cancel);

            // Establish connection and read State
            _client = new AdsClient { Timeout = 5000 };
            _client.Connect(_address);

            ResultReadDeviceState result = await _client.ReadStateAsync(CancellationToken.None);
            _logger.LogInformation($"Target system '{_address}' is in state '{result.State.AdsState}'");

            // Execute the Work handler!
            await OnExecuteAsync(cancel);
        }

        protected abstract Task OnExecuteAsync(CancellationToken cancellationToken);
    }
}