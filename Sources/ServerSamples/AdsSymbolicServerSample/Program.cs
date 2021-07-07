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
            Console.WriteLine("Press the ENTER key to cancel...\n");

            using (SymbolicTestServer server = new SymbolicTestServer())
            {

                Task cancelTask = Task.Run(() =>
                {
                    while (Console.ReadKey().Key != ConsoleKey.Enter)
                    {
                        Console.WriteLine("Press the ENTER key to cancel...");
                    }

                    Console.WriteLine("\nENTER key pressed: cancelling AdsSymbolicServer.\n");
                    s_cts.Cancel();
                });

                Task<AdsErrorCode> serverTask = server.ConnectServerAndWaitAsync(s_cts.Token);
                await Task.WhenAny(new[] { cancelTask, serverTask });
                Console.WriteLine("Application ending.");
            }
        }
    }
}
