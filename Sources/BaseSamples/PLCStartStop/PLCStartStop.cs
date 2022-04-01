using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwinCAT.Ads;

namespace S20_PLCStartStop
{
    class PLCStartStop
    {
        static void Main(string[] args)
        {
            //Create a new instance of class AdsClient
            AdsClient tcClient = new AdsClient();

            try
            {
                //Connect to local PLC - Runtime 1 - Port 851
                tcClient.Connect(851);

                Console.WriteLine(" PLC Run\t[R]");
                Console.WriteLine(" PLC Stop\t[S]");
                Console.WriteLine("\r\nPlease choose \"Run\" or \"Stop\" and confirm with enter..");
                string sInput = Console.ReadLine().ToLower();

                //Process user input and apply chosen state
                do{
                    switch (sInput)
                    {
                        case "r": tcClient.WriteControl(new StateInfo(AdsState.Run, tcClient.ReadState().DeviceState)); break;
                        case "s": tcClient.WriteControl(new StateInfo(AdsState.Stop, tcClient.ReadState().DeviceState)); break;
                        default: Console.WriteLine("Please choose \"Run\" or \"Stop\" and confirm with enter.."); sInput = Console.ReadLine().ToLower(); break;
                    }
                } while (sInput != "r" && sInput != "s");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
            finally
            {
                tcClient.Dispose();
            }
        }
    }
}
