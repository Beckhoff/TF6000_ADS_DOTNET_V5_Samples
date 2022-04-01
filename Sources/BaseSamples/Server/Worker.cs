using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwinCAT.Ads.Server;

namespace S60_Server
{

    public class ServerWorker : BackgroundService
    {
        private readonly ILogger<ServerWorker> _logger;

        public ServerWorker(ILogger<ServerWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancel)
        {
            // Instantiate the server
            AdsSampleServer server = new AdsSampleServer(_logger);
            // Connect the server and wait for cancel
            await server.ConnectServerAndWaitAsync(cancel); 
        }
    }
}
