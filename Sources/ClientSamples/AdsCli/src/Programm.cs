using System;
using TwinCAT.Ams;
using System.Net;
using System.Runtime.CompilerServices;

namespace TwinCAT.Ads.Cli
{
    static class Logger
    {

        static private int logLevel = 1;

        static public void setLogLevel(int level){
            if(level >= 3)
                logLevel = 3;
            else if (level < 0)
                logLevel = 0;
            else
                logLevel = level;
        }

        static public void log(string message, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null){
            if (logLevel <= 0)
                return;

            Console.WriteLine($"{message} - at line: {lineNumber} {caller}");
        }

        static public void logDebug(string message){
            if (logLevel >= 1)
                Console.WriteLine(message);
        }
    }

    class Programm
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static int Main(string[] args)
        {
            ApplicationArgs appArgs = null;
            try
            {
                appArgs = ArgsParser.parse(args);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ArgsParser.printUsage());
                return 1;
            }

            if(appArgs.help){
                Console.WriteLine(ArgsParser.printUsage());
                return 0;
            }

            if(appArgs.version){
                Console.WriteLine(typeof(Programm).Assembly.GetName().Version.ToString());
                return 0;
            }

            IPAddress ipEndpoint = null;
            int port = 48898;
            if(IPAddress.TryParse(System.Environment.GetEnvironmentVariable("AMS_ROUTER_IP_ENDPOINT"), out ipEndpoint))
            {
                int.TryParse(System.Environment.GetEnvironmentVariable("AMS_ROUTER_PORT"), out port);
                Logger.logDebug($"IPEndpoint: {ipEndpoint.ToString()}:{port}");
                AmsConfiguration.RouterEndPoint = new IPEndPoint(ipEndpoint, port);
            }

            using (AdsClient client = new AdsClient())
            {
                try
                {
                    // Connect to Address
                    client.Connect(appArgs.netId, appArgs.port);

                    IAdsCommand cmd = new AdsCommand(client, appArgs.symbolName, appArgs.symbolType, appArgs.value);
                    string result = String.Empty;
                    result = cmd.execute();
                    Console.Write(result);
                    return 0;
                    
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return 1;
                }
            }
        }
    }
}
