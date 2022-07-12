using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.Reactive;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;

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
                // Run the the cancel request in a seperate task asynchronously
                Task cancelTask = Task.Run(() =>
                {
                    while (Console.ReadKey().Key != ConsoleKey.Enter)
                    {
                        Console.WriteLine("\nPress the ENTER key to cancel...\n");
                    }

                    Console.WriteLine("\nENTER key pressed: cancelling AdsSymbolicServer.\n");
                    
                    // Trigger cancel
                    s_cts.Cancel();
                });

                AdsErrorCode errorCode = AdsErrorCode.None;

                // Connect the SymbolicServer in its own task asynchronously
                Task<AdsErrorCode> serverTask = server.ConnectServerAndWaitAsync(s_cts.Token);

                if (serverTask.IsCompleted && serverTask.Result.Failed())
                {
                    Console.WriteLine($"Couldn't start Server (AdsErrorCode: {serverTask.Result})");
                }
                else
                {
                    Console.WriteLine($"Symbolic Test Server runnning on Address: '{server.ServerAddress}' ...\n");
                    Console.WriteLine($"For testing the server see the ReadMe.md file in project root");
                    Console.WriteLine($"or type the following command from Powrshell with installed 'TcXaeMgmt' module:\n");
                    Console.WriteLine($"PS> test-adsroute -NetId {server.ServerAddress.NetId} -port {server.ServerAddress.Port}\n\n");
                    Console.WriteLine("Press the ENTER key to cancel...\n");

                    // Beneath the external access from an out-of-process ADS client
                    // We could also access the server from within this process
                    // What is done in the following for demonstration and testing purposes

                    // Instantiate ADS Session / Client
                    // and execute some operations against the SymbolicServer

                    SessionSettings settings = new SessionSettings(120000);
                    List<IDisposable> disposables = new List<IDisposable>();

                    using (AdsSession session = new AdsSession(AmsNetId.Local, server.ServerPort, settings))
                    {
                        IAdsConnection connection = (IAdsConnection)session.Connection;

                        // Connect to SymbolicServer
                        session.Connect();
                        ISymbolLoader factory = SymbolLoaderFactory.Create(session.Connection, SymbolLoaderSettings.Default);

                        // Load DataTypes in Symbol Instances
                        var types = factory.DataTypes;
                        var symbols = factory.Symbols;

                        // Reading Value By symbol
                        CallReadBySymbol(symbols);

                        // Writing Value By symbol
                        CallWriteValueBySymbol(symbols);

                        // Reading Value by Name/InstancePath (Any_Type)
                        CallReadValueByInstancePath(session);

                        // Writing Value by Name/InstancePath (Any_Type)
                        CallWriteValueByInstancePath(session);

                        // Reading by IndexGroup/IndexOffset
                        CallReadByIndexGroupIndexOffset(session);

                        // Writing by IndexGroup/IndexOffset
                        CallWriteByIndexGroupIndexOffset(session);

                        // Call RPC Methods with different variants of Parameters.
                        CallRpcMethods(symbols);

                        // Receiving notifications on all Symbols
                        SymbolIterator iter = new SymbolIterator(symbols,true);
                        IList<ISymbol> allSymbols = iter.ToList();
                        disposables = ReceiveNotifications(session,allSymbols);

                        // Wait for stopping Server Task or cancellation
                        await Task.WhenAny(new[] { cancelTask, serverTask });
                        errorCode = await serverTask;
                    }
                }

                //AdsErrorCode errorCode = await serverTask;

                if (errorCode.Succeeded())
                {
                    Console.WriteLine("Server stopped without errors.");
                }
                else
                {
                    string message = string.Empty;

                    if (serverTask.Exception != null)
                        message = serverTask.Exception.InnerException.Message;

                    Console.WriteLine($"Server stopped with AdsErrorCode: '{errorCode}' (Exception: {message})");
                }
            }
        }

        private static void CallWriteByIndexGroupIndexOffset(AdsSession session)
        {
            byte[] writeData = new byte[162]; // Encoding.Unicode + /0
            var symbol = session.Connection.ReadSymbol("Main.string1");

            PrimitiveTypeMarshaler.Default.Marshal("WrittenByIGIO", writeData.AsSpan());
            session.Connection.Write(symbol.IndexGroup, symbol.IndexOffset, writeData.AsMemory());
        }

        private static void CallReadByIndexGroupIndexOffset(AdsSession session)
        {
            byte[] data = new byte[162]; // Encoding.Unicode + /0
            var adsSymbol = session.Connection.ReadSymbol("Main.string1");
            session.Connection.Read(adsSymbol.IndexGroup, adsSymbol.IndexOffset, data.AsMemory());
            PrimitiveTypeMarshaler.Default.Unmarshal(data, Encoding.Unicode, out var s2);
        }

        private static void CallWriteValueByInstancePath(AdsSession session)
        {
            string value = "WrittenByName";
            session.Connection.WriteValue("Main.string1", value);
        }

        private static void CallReadValueByInstancePath(AdsSession session)
        {
            bool bValue = (bool) session.Connection.ReadValue("Main.bool1", typeof(bool));
            string sValue = (string) session.Connection.ReadValue("Main.string1", typeof(string));
        }

        private static void CallWriteValueBySymbol(ISymbolCollection<ISymbol> symbols)
        {
            //var bSymbol = (IValueSymbol) symbols["Main.bool1"];
            var sSymbol = (IValueSymbol) symbols["Main.string1"];
            string sValue = "WrittenBySymbol";
            sSymbol.WriteValue(sValue);
        }

        private static void CallReadBySymbol(ISymbolCollection<ISymbol> symbols)
        {
            var bSymbol = (IValueSymbol) symbols["Main.bool1"];
            var sSymbol = (IValueSymbol) symbols["Main.string1"];
            bool bValue = (bool) bSymbol.ReadValue();
            string sValue = (string) sSymbol.ReadValue();
        }

        private static void CallRpcMethods(ISymbolCollection<ISymbol> symbols)
        {
            symbols.TryGetInstance("Main.rpcInvoke1", out var s);

            IInterfaceInstance rpcInvoke = (IInterfaceInstance)s; // Could also be IStructInstance

            // INT Method1([in] INT i1, [in] i2)
            object m1ReturnValue = rpcInvoke.InvokeRpcMethod("Method1", new object[] { (short)44, (short)55 });

            // INT Method2([in] INT in1, [out] INT out1)
            object[] m2OutParameters = new object[1];
            object m2ReturnValue = rpcInvoke.InvokeRpcMethod("Method2", new object[] { (short)43 }, out m2OutParameters);

            // STRING[80] Method3([in] INT len, [in][LengthIs = 1] PCCH str)
            string m3Value = "CallToMethod3";
            byte[] m3ValueData = Encoding.UTF8.GetBytes(m3Value);
            object[] m3InParameters = new object[] { (short)3, m3ValueData };
            object result3 = rpcInvoke.InvokeRpcMethod("Method3", m3InParameters);

            // STRING[80] Method4([in] INT len, [out][LengthIs = 1] PCCH str)
            object[] m4OutParameters = new object[2];
            object m4ReturnValue = rpcInvoke.InvokeRpcMethod("Method4", new object[] { (short)7 }, out m4OutParameters);

            // STRING[80] Method5([in] INT len, [out][LengthIs = 1] PCCH str)
            object[] m5OutParameters = new object[1];
            object m5ReturnValue = rpcInvoke.InvokeRpcMethod("Method5", new object[] { (short)14 }, out m5OutParameters);
        }

        private static List<IDisposable> ReceiveNotifications(AdsSession session, IList<ISymbol> symbols)
        {
            List<IDisposable> result = new List<IDisposable>();

            IDisposable subscription = SubscribeNotifications(session, symbols, NotificationSettings.Default);
            
            // Take care not to GC the subscription - keeping notifications alive.
            result.Add(subscription);
            return result;
        }


        /// <summary>
        /// Subscribe to Notifications and Write Value changes into the Console output.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="symbols">The symbols.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>IDisposable subscription.</returns>
        private static IDisposable SubscribeNotifications(AdsSession session, IList<ISymbol> symbols, NotificationSettings settings)
        {
            var observable = session.Connection
                .WhenNotification(symbols, settings);
            //.Take(10);

            var subscription = observable.Subscribe(
                c => Console.WriteLine($"WhenNotification Symbol '{c.Symbol.InstancePath}' changed to value '{c.Value}'"),
                ex => Console.WriteLine($"WhenNotification failed: {ex.Message}"),
                () => Console.WriteLine($"WhenNotification completed!")
                );

            return subscription;
        }
    }
}
