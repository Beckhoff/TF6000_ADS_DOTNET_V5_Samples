using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwinCAT.Ads;

namespace TwinCAT.Ads.Cli
{
    public static class ArgsParser
    {
        /// <summary>
        /// Parses the arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>AmsAddress.</returns>
        public static ApplicationArgs parse(string[] args)
        {

            if((args == null) || (args.Length == 0)){
                throw new Exception("No arguments passed");
            }

            ApplicationArgs appArgs = null;
            if(args.Length == 1)
            {
                switch (args[0])
                {
                    case "--help":
                    case "-h": 
                    appArgs = new ApplicationArgs(help: true);
                    break;

                    case "--version":
                    appArgs = new ApplicationArgs(version: true);
                    break;

                    default: throw new Exception($"Unknwon argument: ${args[0]}");
                }
            }
            else if(args.Length > 1)
            {
                appArgs = new ApplicationArgs(
                    netId: tryParseNetId(args[0]),
                    port: tryParsePort(args[0]),
                    symbolType: tryParseType(args[1]),
                    symbolName: tryParseSymbol(args[2]),
                    value: tryParseValue(args[3]));
            }

            return appArgs;
        }

        public static string printUsage(){
            return @"
ads <NetID[:Port=851]> <Type> <Symbol> [Value] 

PARAMS:
    NetID           The AMS target NetId
    Port            The optional AMS target port. Per default port 851 is set. Use the colon to seperate NetID and Port
    Type            PLC variable type of the given <SymbolyName>
    SymbolName      Specifies the ADS symbol name to be read or written

OPTIONS:
    Value           If given, the value will be written to the passed symbol name

EXAMPLES:
    Read examples:
        ads 5.76.88.215.1.1 'STRING(80)' 'CONST.VERSION'
    Write examples:
        ads 5.76.88.215.1.1 'INT' 'MAIN.counter' '16'
        ads 5.76.88.215.1.1 'STRING(80)' 'MAIN.text' 'beckhoff'
";
        }

        private static AmsNetId tryParseNetId(string arg)
        {
            return AmsNetId.Parse(arg.Split(':')[0]);
        }
        private static int tryParsePort(string arg)
        {
            string[] argValue = arg.Split(':');
            if(argValue.Length == 1){
                return 851;
            }

            int port = Convert.ToInt32(argValue[1]);
            return port;
        }
        private static string tryParseType(string arg)
        {
            if(String.IsNullOrEmpty(arg)){
                throw new Exception("Missing <Type> argument");
            }
            return arg.ToLower();
        }
        private static string tryParseSymbol(string arg)
        {
            if(String.IsNullOrEmpty(arg)){
                throw new Exception("Missing <Symbol> argument");
            }
            return arg;
        }
        private static string tryParseValue(string arg)
        {
            if(String.IsNullOrEmpty(arg)){
                return null;
            }
            return arg;
        }
    }

    public class ApplicationArgs
    {
        public ApplicationArgs(bool help = false, bool version = false){
            this.help = help;
            this.version = version;
        }
        public ApplicationArgs(AmsNetId netId, int port, string symbolType, string symbolName, string value)
        {
            this.netId = netId;
            this.port = port;
            this.symbolType = symbolType;
            this.symbolName = symbolName;
            this.value = value;
            this.help = false;
            this.version = false;
        }

        public AmsNetId netId {get;}
        public int port {get;}
        public string symbolType {get;}
        public string symbolName {get;}
        public string value {get;}
        public bool help {get;}
        public bool version {get;}
    }
}
