using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestServer;

namespace Sample.Ads.AdsServerCore
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
            //ServerLogger logger = new ServerLogger(s_logger);
            AdsSampleServer server1 = new AdsSampleServer(30000, "TestAdsServer1", _logger);
            AdsSampleServer server2 = new AdsSampleServer(30001, "TestAdsServer2", _logger);

            Task[] serverTasks = new Task[2];

            serverTasks[0] = server1.ConnectServerAndWaitAsync(cancel);
            serverTasks[1] = server2.ConnectServerAndWaitAsync(cancel);

            Task shutdownTask = Task.Run(async () =>
            {
                await Task.WhenAll(serverTasks);
                _logger.LogInformation("All AdsServers closed down.!");
            });

            Console.WriteLine("Press enter to shutdown servers ...");
            Console.ReadLine();

            server1.Disconnect();
            server2.Disconnect();

            await shutdownTask; // Wait for Shutdown of both Servers
        }
    }
}
