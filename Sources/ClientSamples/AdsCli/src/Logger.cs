using System;

namespace TwinCAT.Ads.Cli
{
    public static class Logger
    {
        public static bool enableLogging;

        public static void log(string message){
            if(! enableLogging)
                return;

            DateTime now = DateTime.UtcNow;

            Console.WriteLine($"{now:O}: {message}");
        }
    }
}