using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TwinCAT.Ads;

namespace S02_AccessByVarName
{
    class AccessByVarName
    {
        static void Main(string[] args)
        {
            //Create a new instance of class AdsClient
            AdsClient tcClient = new AdsClient();

            uint iHandle = 0;
            uint iValue = 0;
            
            try
            {
                //Connect to local PLC - Port 851
                tcClient.Connect(851);
                
                //Get the handle of the PLC variable "nCounter"
                iHandle = tcClient.CreateVariableHandle("MAIN.nCounter");
                Console.WriteLine("Press Enter five times to end");
                for(int i = 0; i < 5; i++)
                {
                    //Use the handle to read PLCVar
                    byte[] readData = new byte[sizeof(UInt32)];
                    tcClient.Read(iHandle, readData.AsMemory());
                    MemoryStream dataStream = new MemoryStream(readData);
                    BinaryReader binReader = new BinaryReader(dataStream);
                    iValue = binReader.ReadUInt32();
                    Console.WriteLine("Value: " + iValue);
                    Console.ReadKey();
                }

                //Reset PLC variable to zero
                tcClient.WriteAny(iHandle, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
            finally
            {
                tcClient.DeleteVariableHandle(iHandle);
                tcClient.Dispose();
            }
        }
    }
}
