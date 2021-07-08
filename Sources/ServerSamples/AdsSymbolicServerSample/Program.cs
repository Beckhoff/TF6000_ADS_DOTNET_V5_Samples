using System;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads;

namespace AdsSymbolicServerSample
{
    class Program
    {
        static readonly CancellationTokenSource s_cts = new CancellationTokenSource();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting the AdsSymbolicServer ...\n");

            using (SymbolicTestServer server = new SymbolicTestServer())
            {

                Task cancelTask = Task.Run(() =>
                {
                    while (Console.ReadKey().Key != ConsoleKey.Enter)
                    {
                        Console.WriteLine("\nPress the ENTER key to cancel...\n");
                    }

                    Console.WriteLine("\nENTER key pressed: cancelling AdsSymbolicServer.\n");
                    s_cts.Cancel();
                });

                Task<AdsErrorCode> serverTask = server.ConnectServerAndWaitAsync(s_cts.Token);
                Console.WriteLine($"Symbolic Test Server runnning on Address: '{server.ServerAddress}' ...\n");
                Console.WriteLine($"For testing the server see the ReadMe.md file in project root");
                Console.WriteLine($"or type the following command from Powrshell with installed 'TcXaeMgmt' module:\n");
                Console.WriteLine($"PS> test-adsroute -NetId {server.ServerAddress.NetId} -port {server.ServerAddress.Port}\n\n");
                Console.WriteLine("Press the ENTER key to cancel...\n");

                await Task.WhenAny(new[] { cancelTask, serverTask });
                Console.WriteLine("Application ending.");
            }
        }
    }
}
