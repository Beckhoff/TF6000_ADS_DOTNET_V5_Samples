using System;
using System.Buffers.Binary;
using TwinCAT.TypeSystem;

namespace TwinCAT.Ads.Cli
{
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

        private int sizeOfType(string type)
        {
            string _type = type.ToLower();
            int result = 0;

            if (_type.StartsWith("string"))
            {
                if (_type.Contains('(')){
                    string size = _type.Split('(',2,StringSplitOptions.TrimEntries)[1].Replace(")","");
                    int.TryParse(size, System.Globalization.NumberStyles.Integer, null, out result);
                }
                else {
                    result = 80;
                }
            } else {
                switch(_type){
                    case "bool":
                        result = 1;
                        break;
                    case "int":
                    case "uint": 
                    case "word":
                        result = 2;
                        break;
                    case "dword":
                    case "real":
                    case "dint":
                        result = 4;
                        break;
                    case "lreal":
                        result = 8;
                        break;
                    default:
                        throw new Exception($"Unknwon type: {type}");
                }
            }

            Logger.log($"Expected size of type {type} is {result} bytes");
            return result;
        }

        private string convertBuffer(string type, byte[] buffer)
        {
            string _type = type.ToLower();
            byte[] _buffer = buffer;
            string result = string.Empty;

            Logger.log($"Buffer data:{_buffer.ToString()}");

            if (_type.StartsWith("string"))
            {
                PrimitiveTypeMarshaler marshaler = PrimitiveTypeMarshaler.Default;
                int unmarshaledBytes = marshaler.Unmarshal(_buffer, _client.DefaultValueEncoding, out result);
            } else {
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

            Logger.log($"Converted buffer data: {result}");
            return result;
        }

        private void convertValue(string value, string type, ref byte[] buffer)
        {
            string _type = type.ToLower();
            if (_type.StartsWith("string"))
            {
                PrimitiveTypeMarshaler marshaler = PrimitiveTypeMarshaler.Default;
                int unmarshaledBytes = marshaler.Marshal(AdsDataTypeId.ADST_STRING, value, buffer);
            } else {
                switch(_type){
                    case "bool":
                        buffer[0] = value.Equals("0") ? (byte)0 : (byte)1;
                        break;
                    case "int":
                    case "word":
                        BinaryPrimitives.WriteInt16LittleEndian(buffer, short.Parse(value));
                        break;
                    case "uint":
                        BinaryPrimitives.WriteUInt16LittleEndian(buffer, UInt16.Parse(value));
                        break;
                    case "dint":
                    case "dword":
                    case "real":
                        BinaryPrimitives.WriteSingleLittleEndian(buffer, float.Parse(value));
                        break;
                    case "lreal":
                        BinaryPrimitives.WriteDoubleLittleEndian(buffer, double.Parse(value));
                        break;
                    default:
                        throw new Exception($"Unknwon type: {type}");
                }
            }

            Logger.log($"Converted value {value} of type {type} into buffer: {System.BitConverter.ToString(buffer)}");
        }
        private string read()
        {
            string result = "";
            uint handle = 0;
            try
            {
                Logger.log($"Will read symbol {_symbol} of type {_type} with size {sizeOfType(_type)}");
                byte[] buffer = new byte[sizeOfType(_type)];
                handle = _client.CreateVariableHandle(_symbol);
                _client.Read(handle, buffer.AsMemory());
                Logger.log($"Received data: {System.BitConverter.ToString(buffer)}");
                result = convertBuffer(_type, buffer);
                Logger.log($"Converted data: {result}");
            }catch(Exception e)
            {
                Logger.log(e.Message);
                Logger.log($"Could not read {_symbol} of size {sizeOfType(_type)}");
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
                byte[] buffer = new byte[sizeOfType(_type)];
                convertValue(_value, _type, ref buffer);
                Logger.log($"write buffer: {System.BitConverter.ToString(buffer)}");
                handle = _client.CreateVariableHandle(_symbol);
                Logger.log($"Will write symbol {_symbol} of type {_type} with value {_value}");
                _client.Write(handle, buffer);
                Logger.log($"Symbol successfully written");
            }
            catch (Exception e)
            {
                Logger.log(e.Message);
                Logger.log($"Could not write symbol");
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