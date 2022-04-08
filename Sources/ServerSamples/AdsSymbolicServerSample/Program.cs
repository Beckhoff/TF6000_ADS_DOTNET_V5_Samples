using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT;
using TwinCAT.Ads;
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
                //await Task.Delay(500);

                if (serverTask.IsCompleted && serverTask.Result.Failed())
                {
                    Console.WriteLine("Couldn't start Server");
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
                    SessionSettings settings = new SessionSettings(120000);
                    using (AdsSession session = new AdsSession(AmsNetId.Local, server.ServerPort, settings))
                    {
                        IAdsConnection connection = (IAdsConnection)session.Connection;

                        session.Connect();
                        ISymbolLoader factory = SymbolLoaderFactory.Create(session.Connection, SymbolLoaderSettings.Default);

                        // Access Symbols
                        var types = factory.DataTypes;
                        var symbols = factory.Symbols;

                        // Reading Value By symbol
                        var sym1 = (IValueSymbol)symbols["Main.bool1"];
                        var sym2 = (IValueSymbol)symbols["Main.string1"];
                        bool val1 = (bool)sym1.ReadValue();
                        string val2 = (string)sym2.ReadValue();

                        // Writing Value By symbol
                        val2 = "WrittenBySymbol";
                        sym2.WriteValue(val2);

                        // Reading Value by Name/InstancePath (Any_Type)
                        bool b1 = (bool)session.Connection.ReadValue("Main.bool1", typeof(bool));
                        string s1 = (string) session.Connection.ReadValue("Main.string1", typeof(string));

                        // Reading Value by Name/InstancePath (Any_Type)
                        s1 = "WrittenByName";
                        session.Connection.WriteValue("Main.string1",s1);

                        // Reading by IndexGroup/IndexOffset
                        byte[] data = new byte[162];
                        var adsSymbol = session.Connection.ReadSymbol("Main.string1");
                        session.Connection.Read(adsSymbol.IndexGroup, adsSymbol.IndexOffset, data.AsMemory());
                        PrimitiveTypeMarshaler.Default.Unmarshal(data, Encoding.Unicode, out var s2);

                        // Writing by IndexGroup/IndexOffset
                        byte[] writeData = new byte[162];
                        PrimitiveTypeMarshaler.Default.Marshal("WrittingByIGIO", writeData.AsSpan());
                        session.Connection.Write(adsSymbol.IndexGroup, adsSymbol.IndexOffset,writeData.AsMemory() );

                        // Call RPC Methods with different variants of Parameters.

                        symbols.TryGetInstance("Main.rpcInvoke1", out var s);

                        IRpcStructInstance rpcInvoke = (IRpcStructInstance)s;

                        // INT Method1([in] INT i1, [in] i2)
                        object result = rpcInvoke.InvokeRpcMethod("Method1", new object[] { (short)44, (short)55 });

                        // INT Method2([in] INT in1, [out] INT out1)
                        object[] outParameters2 = new object[1];
                        object result2 = rpcInvoke.InvokeRpcMethod("Method2", new object[] { (short)43 }, out outParameters2);

                        // STRING[80] Method3([in] INT len, [in][LengthIs = 1] PCCH str)
                        string method3Value = "CallToMethod3";
                        byte[] method3Data = Encoding.UTF8.GetBytes(method3Value);
                        object[] inParameters3 = new object[] { (short)3, method3Data };
                        object result3 = rpcInvoke.InvokeRpcMethod("Method3", inParameters3);

                        // STRING[80] Method4([in] INT len, [out][LengthIs = 1] PCCH str)
                        object[] outParameters4 = new object[2];
                        object result4 = rpcInvoke.InvokeRpcMethod("Method4", new object[] { (short)7 }, out outParameters4);

                        // STRING[80] Method5([in] INT len, [out][LengthIs = 1] PCCH str)
                        object[] outParameters5 = new object[1];
                        object result5 = rpcInvoke.InvokeRpcMethod("Method5", new object[] { (short)14 }, out outParameters5);
                    }
                }

                await Task.WhenAny(new[] { cancelTask, serverTask });
                
                AdsErrorCode errorCode = await serverTask;

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
    }
}
