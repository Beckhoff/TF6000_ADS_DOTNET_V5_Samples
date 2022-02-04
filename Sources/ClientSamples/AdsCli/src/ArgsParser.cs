using System;
using System.Linq;

namespace TwinCAT.Ads.Cli
{
    public static class ArgsParser
    {

        private static string[] _args;

        public static ApplicationArgs parse(string[] args)
        {
            _args = args;

            if((_args == null) || (_args.Length == 0)){
                throw new Exception("No arguments passed");
            }

            ApplicationArgs appArgs = null;

            if(_args.Length == 1)
            {
                switch (_args[0])
                {
                    case "--help":
                    case "-h": 
                    appArgs = new ApplicationArgs(help: true);
                    break;

                    case "--version":
                    appArgs = new ApplicationArgs(version: true);
                    break;

                    default: throw new Exception($"Unknwon argument: ${_args[0]}");
                }
            }
            else if(_args.Length > 1)
            {
                bool verbosity = isVerboseSet();
                string setValue = null;

                if(_args.Length > 3){
                    setValue = tryParseValue(_args[3]);
                }

                appArgs = new ApplicationArgs(
                    netId: tryParseNetId(_args[0]),
                    port: tryParsePort(_args[0]),
                    symbolType: tryParseType(_args[1]),
                    symbolName: tryParseSymbol(_args[2]),
                    value: setValue,
                    verbosity: verbosity);
            }

            return appArgs;
        }

        public static string printUsage(){
            return @"
ads [OPTIONS] <NetID[:Port=851]> <Type> <SymbolName> [Value] 

PARAMS:
    NetID           The AMS target NetId
    Port            The optional AMS target port. Per default port 851 is set. Use the colon to seperate NetID and Port
    Type            PLC variable type of the given <SymbolyName>
    SymbolName      Specifies the ADS symbol name to be read or written
    Value           If given, the value will be written to the set <SymbolyName>

OPTIONS:
    -v, --verbose   enable verbose log output

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

        private static bool isVerboseSet()
        {
            bool isVerbose = false;
            if(_args.Contains("-v"))
            {
                _args = _args.Where( item => item != "-v").ToArray();
                isVerbose = true;
            }
            if(_args.Contains("--verbose")){
                _args = _args.Where( item => item != "--verbose").ToArray();
                isVerbose = true;
            }

            return isVerbose;
        }
    }

    public class ApplicationArgs
    {
        public ApplicationArgs(bool help = false, bool version = false, bool verbosity = false){
            this.help = help;
            this.version = version;
            this.verbosity = verbosity;
        }
        public ApplicationArgs(AmsNetId netId, int port, string symbolType, string symbolName, string value, bool verbosity = false)
        {
            this.netId = netId;
            this.port = port;
            this.symbolType = symbolType;
            this.symbolName = symbolName;
            this.value = value;
            this.help = false;
            this.version = false;
            this.verbosity = verbosity;
        }

        public AmsNetId netId {get;}
        public int port {get;}
        public string symbolType {get;}
        public string symbolName {get;}
        public string value {get;}
        public bool help {get;}
        public bool version {get;}
        public bool verbosity {get;}
    }
}
