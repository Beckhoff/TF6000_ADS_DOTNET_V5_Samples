#region CODE_SAMPLE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

using TwinCAT.Ads;

namespace HeartbeatTest
{

    class Program
    {
        static void Main(string[] args)
        {
            // Connect to local system service
            AmsNetId netId = AmsNetId.Local;
            AmsPort port = AmsPort.SystemService;

            bool useSessions = false;
            IAdsConnection connection = null;

            if (useSessions)
            {
                SessionSettings settings = new SessionSettings(200);

                // Use Session
                AdsSession session = new AdsSession(new AmsAddress(netId, 10000), settings);
                connection = (AdsConnection)session.Connect();
            }
            else
            {
                // Use Raw AdsClient
                AdsClientSettings settings = new AdsClientSettings(200);
                AdsClient client = new AdsClient(settings);
                connection = (IAdsConnection)client;
                client.Connect(netId, port);
            }

            // Implementation of an ADS Heartbeat (using Reactive Extensions)
            Observable.Interval(TimeSpan.FromMilliseconds(200)) // Trigger every 200 ms
                .Select(i =>                                    // Read State on each event
                {
                    StateInfo state;
                    AdsErrorCode errorCode = connection.TryReadState(out state);
                    return state.AdsState;
                })
                //.DistinctUntilChanged()                       // Produce only distinct values
                .SubscribeConsole();                            // Publish to Console

            Console.ReadLine();
        }
    }

    /// <summary>
    /// Extension to subscribe ConsoleObserver to Observables (for fluent API)
    /// </summary>
    public static class Extensions
    {
        public static IDisposable SubscribeConsole<T>(
            this IObservable<T> observable)
        {
            return observable.Subscribe(new ConsoleObserver<T>()); // Subscribe ConsoleObserver to Observable
        }
    }

    /// <summary>
    /// Helper observer for Console output
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.IObserver{T}" />
    public class ConsoleObserver<T> : IObserver<T>
    {
        public void OnCompleted()
        {
            Console.WriteLine("OnCompleted");
        }

        public void OnError(Exception error)
        {
            Console.WriteLine("OnError: {0}", error.Message);
        }

        public void OnNext(T value)
        {
            Console.WriteLine("OnNext: {0}", value.ToString());
        }
    }
}
#endregion
