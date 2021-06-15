using System;
using System.Buffers.Binary;
using TwinCAT.Ads;
using TwinCAT.TypeSystem;
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
    class AdsCliClient
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static int Main(string[] args)
        {
            #if (DEBUG)
                Logger.setLogLevel(1);
            #endif

            if (args.Length == 1 && (args[0].Equals("-h") || args[0].Equals("--help") ) )
            {
                printHelp();
                return 0;
            }
            if (args.Length < 3 ){
                Console.WriteLine("ERROR: Wrong set of command arugments.\nUsage:");
                printHelp();
                return 1;
            }

            IPAddress ipEndpoint = null;
            int port = 48898;
            if(IPAddress.TryParse(System.Environment.GetEnvironmentVariable("AMS_ROUTER_IP_ENDPOINT"), out ipEndpoint))
            {
                int.TryParse(System.Environment.GetEnvironmentVariable("AMS_ROUTER_PORT"), out port);
                Logger.logDebug($"IPEndpoint: {ipEndpoint.ToString()}:{port}");
                AmsConfiguration.RouterEndPoint = new IPEndPoint(ipEndpoint, port);
            }

            AmsAddress address = ArgParser.Parse(args[0].Split(':'));
            string plcType = args[1];
            string plcSymbol = args[2];
            string setValue = args.Length > 3 ? args[3] : null;
            int returnValue = 1;
            string result = String.Empty;

            using (AdsClient client = new AdsClient())
            {
                try
                {
                    // Connect to Address
                    Logger.logDebug($"Try to connect to: {address.ToString()}");
                    client.Connect(address.NetId, address.Port); // Connect to Port (851, first PLC by default)

                    AdsCommand cmd = new AdsCommand(client, plcSymbol, plcType, setValue);
                    result = cmd.execute();
                    returnValue = 0;
                    
                }
                catch (System.Exception ex)
                {
                    Logger.log(ex.ToString());
                }
            }

            Console.Write(result);
            return returnValue;
        }

        private static void printHelp()
        {
            string help = @"
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

        Console.WriteLine(help);
        }
    }

    interface IAdsCommand
    {
        string execute();
    }

    class AdsCommand : IAdsCommand
    {
        protected AdsClient _client;
        protected string _type;
        protected string _symbol;
        protected string _value;

        public AdsCommand(AdsClient client, string symbol, string type, string value=null)
        {
            _client = client;
            _type = type;
            _symbol = symbol;
            _value = value;
        }

        public string execute(){
            return _value != null ? write() : read();
        }

        private int sizeOfType(string type){
            string _type = type.ToLower();
            int result = 0;

            if (_type.StartsWith("string"))
            {
                if (! _type.Contains('('))
                    return 80;
                
                string size = _type.Split('(',2,StringSplitOptions.TrimEntries)[1].Replace(")","");
                Logger.logDebug($"sizeOfType: {type}\t{size}");
                int.TryParse(size, System.Globalization.NumberStyles.Integer, null, out result);
                Logger.logDebug($"sizeOfType: {type}\t{result}");
                return result;
            }

            switch(_type){
                case "bool":
                    return 1;
                case "int":
                case "uint": 
                case "word":
                    return 2;
                case "dword":
                case "real":
                case "dint":
                    return 4;
                case "lreal":
                    return 8;
                default:
                    throw new Exception($"Unknwon type: {type}");
            }
        }

        private string convertBuffer(string type, byte[] buffer)
        {
            string _type = type.ToLower();
            byte[] _buffer = buffer;

            if (_type.StartsWith("string"))
            {
                PrimitiveTypeMarshaler marshaler = PrimitiveTypeMarshaler.Default;
                string result = null;
                int unmarshaledBytes = marshaler.Unmarshal(_buffer, _client.DefaultValueEncoding, out result);
                return result;
            }

            switch(_type){
                case "bool":
                    return _buffer[0] > 0 ? "1" : "0";
                case "int":
                case "word":
                    return BinaryPrimitives.ReadInt16LittleEndian(_buffer).ToString();
                case "uint":
                    return BinaryPrimitives.ReadUInt16LittleEndian(_buffer).ToString();
                case "dint":
                case "dword":
                case "real":
                    return BinaryPrimitives.ReadSingleLittleEndian(_buffer).ToString();
                case "lreal":
                    return BinaryPrimitives.ReadDoubleLittleEndian(_buffer).ToString();
                default:
                    throw new Exception($"Unknwon type: {type}");
            }

        }

        private void convertValue(string value, string type, ref byte[] buffer)
        {
            string _type = type.ToLower();
            if (_type.StartsWith("string"))
            {
                PrimitiveTypeMarshaler marshaler = PrimitiveTypeMarshaler.Default;
                int unmarshaledBytes = marshaler.Marshal(AdsDataTypeId.ADST_STRING, value, buffer);
                return;
            }

            switch(_type){
                case "bool":
                    buffer[0] = value.Equals("0") ? (byte)0 : (byte)1; return;
                case "int":
                case "word":
                    BinaryPrimitives.WriteInt16LittleEndian(buffer, short.Parse(value)); return;
                case "uint":
                    BinaryPrimitives.WriteUInt16LittleEndian(buffer, UInt16.Parse(value)); return;
                case "dint":
                case "dword":
                case "real":
                    BinaryPrimitives.WriteSingleLittleEndian(buffer, float.Parse(value)); return;
                case "lreal":
                    BinaryPrimitives.WriteDoubleLittleEndian(buffer, double.Parse(value)); return;
                default:
                    throw new Exception($"Unknwon type: {type}");
            }
        }
        private string read(){
            string result = "";
            uint handle = 0;
            try
            {
                byte[] buffer = new byte[sizeOfType(_type)];
                handle = _client.CreateVariableHandle(_symbol);
                _client.Read(handle, buffer.AsMemory());
                result = convertBuffer(_type, buffer);
            }catch(Exception e)
            {
                Logger.log(e.Message);
                Logger.logDebug($"Could not read: {_symbol} of size {sizeOfType(_type)}");
            }finally
            {
                if(handle != 0)
                    _client.DeleteVariableHandle(handle);
            }

            return result+"\n";
        }

        private string write()
        {
            uint handle = 0;
            try
            {
                Logger.logDebug($"Going to write: {_type} {_symbol} {_value}");
                byte[] buffer = new byte[sizeOfType(_type)];
                convertValue(_value, _type, ref buffer);
                Logger.logDebug($"write buffer: {System.BitConverter.ToString(buffer)}");
                handle = _client.CreateVariableHandle(_symbol);
                _client.Write(handle, buffer);
            }
            catch (Exception e)
            {
                Logger.log(e.Message);
                Logger.logDebug($"Could not write: {_symbol} of size {sizeOfType(_type)} with value {_value}");
                throw;
            }
            finally
            {
                if (handle != 0)
                    _client.DeleteVariableHandle(handle);
            }

            return string.Empty;
        }
    }
}
