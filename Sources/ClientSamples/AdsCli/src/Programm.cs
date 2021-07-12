using System;
using TwinCAT.Ams;
using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace TwinCAT.Ads.Cli
{
    class Programm
    {
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

            Logger.enableLogging = appArgs.verbosity;

            IPAddress ipEndpoint;
            if(IPAddress.TryParse(System.Environment.GetEnvironmentVariable("AMS_ROUTER_IP_ENDPOINT"), out ipEndpoint))
            {
                Logger.log($"AMS_ROUTER_IP_ENDPOINT={ipEndpoint.ToString()}");
            } else {
                ipEndpoint = IPAddress.Loopback;
            }

            int port;
            if(int.TryParse(System.Environment.GetEnvironmentVariable("AMS_ROUTER_PORT"), out port))
            {
                Logger.log($"AMS_ROUTER_PORT={port.ToString()}");
            } else {
                port = 48898;
            }

            AmsConfiguration.RouterEndPoint = new IPEndPoint( ipEndpoint, port);
            Logger.log($"AMS router endpoint set to: {AmsConfiguration.RouterEndPoint.ToString()}");

            using (AdsClient client = new AdsClient())
            {
                try
                {
                    // Connect to Address
                    Logger.log($"ADS client will connect to ADS service: {appArgs.netId}:{appArgs.port}");
                    client.Connect(appArgs.netId, appArgs.port);
                    Logger.log($"ADS client connected to ADS service: {appArgs.netId}:{appArgs.port}");

                    IAdsCommand cmd = new AdsCommand(client, appArgs.symbolName, appArgs.symbolType, appArgs.value);
                    string result = String.Empty;
                    result = cmd.execute();
                    Console.Write(result);
                    return 0;
                    
                }
                catch (System.Exception ex)
                {
                    Logger.log(ex.Message);
                    return 1;
                }
            }
        }
    }
}
