RouterConsole:
Clone the Router console App from:
```pwsh
git clone https://github.com/Beckhoff/TF6000_ADS_DOTNET_V5_Samples.git
cd .\TF6000_ADS_DOTNET_V5_Samples\Sources\RouterSamples\AdsRouterConsoleApp\src
dotnet build
```

Start with Default parameters (are taken from the appsettings.json)

```pwsh
PS> dotnet run

Application Directories
=======================
ApplicationPath: D:\tmp\githubtest\TF6000_ADS_DOTNET_V5_Samples\Sources\RouterSamples\AdsRouterConsoleApp\src\bin\Debug\net8.0\AdsRouterConsoleApp.dll
BaseDirectory: D:\tmp\githubtest\TF6000_ADS_DOTNET_V5_Samples\Sources\RouterSamples\AdsRouterConsoleApp\src\bin\Debug\net8.0\
CurrentDirectory: D:\tmp\githubtest\TF6000_ADS_DOTNET_V5_Samples\Sources\RouterSamples\AdsRouterConsoleApp\src


Configuration
=============
ASPNETCORE_ENVIRONMENT: Production


Press Ctrl + C to shutdown!
[2025-10-17 16:50:19.003] [Thread:1] Information: TwinCAT.Ads.TcpRouter.AmsTcpIpRouter Allowed EXTERNAL LOOPBACK NETWORK: 0.0.0.0/0
[2025-10-17 16:50:19.223] [Thread:1] Information: TwinCAT.Ads.TcpRouter.AmsTcpIpRouter Local System Name: LocalSystem
[2025-10-17 16:50:19.224] [Thread:1] Information: TwinCAT.Ads.TcpRouter.AmsTcpIpRouter Local AmsNetId:    1.1.1.1.1.1
[2025-10-17 16:50:19.224] [Thread:1] Information: TwinCAT.Ads.TcpRouter.AmsTcpIpRouter IPAddresses:       169.254.253.168,192.168.56.1,172.17.60.232,172.30.224.1
[2025-10-17 16:50:19.224] [Thread:1] Information: TwinCAT.Ads.TcpRouter.AmsTcpIpRouter External Port:     bf02
[2025-10-17 16:50:19.224] [Thread:1] Information: TwinCAT.Ads.TcpRouter.AmsTcpIpRouter Loopback IP:       127.0.0.1
[2025-10-17 16:50:19.225] [Thread:1] Information: TwinCAT.Ads.TcpRouter.AmsTcpIpRouter Loopback Port:     bf02
[2025-10-17 16:50:19.225] [Thread:1] Information: TwinCAT.Ads.TcpRouter.AmsTcpIpRouter UDP Discovery Port:bf03
[2025-10-17 16:50:19.225] [Thread:1] Information: TwinCAT.Ads.TcpRouter.AmsTcpIpRouter LoopbackExternals: 0.0.0.0/0
[2025-10-17 16:50:19.225] [Thread:1] Information: TwinCAT.Ads.TcpRouter.AmsTcpIpRouter
[2025-10-17 16:50:19.225] [Thread:1] Information: TwinCAT.Ads.TcpRouter.AmsTcpIpRouter Configured routes:
[2025-10-17 16:50:19.225] [Thread:1] Information: TwinCAT.Ads.TcpRouter.AmsTcpIpRouter ==================
[2025-10-17 16:50:19.225] [Thread:1] Information: TwinCAT.Ads.TcpRouter.AmsTcpIpRouter  RemoteSystem, 2.2.2.2.1.1, 192.168.0.2
[2025-10-17 16:50:19.225] [Thread:1] Information: TwinCAT.Ads.TcpRouter.AmsTcpIpRouter
[2025-10-17 16:50:19.225] [Thread:1] Information: TwinCAT.Ads.AdsRouterService.RouterService ApplicationPath: D:\tmp\githubtest\TF6000_ADS_DOTNET_V5_Samples\Sources\RouterSamples\AdsRouterConsoleApp\src\bin\Debug\net8.0\AdsRouterConsoleApp.dll
BaseDirectory: D:\tmp\githubtest\TF6000_ADS_DOTNET_V5_Samples\Sources\RouterSamples\AdsRouterConsoleApp\src\bin\Debug\net8.0\
CurrentDirectory: D:\tmp\githubtest\TF6000_ADS_DOTNET_V5_Samples\Sources\RouterSamples\AdsRouterConsoleApp\src
```

Be shure the settings look like this (to replace the standard Twincat Router with default network interfaces):

```pwsh
IPAddresses:       169.254.253.168,192.168.56.1,172.17.60.232,172.30.224.1
External Port:     bf02
Loopback IP:       127.0.0.1
Loopback Port:     bf02
```

As next step the remote route to the remote plc must be registered:
Open Powershell (here we use the Powershell TcXaeMgmt Module - this must be preinstalled)


```pwsh
PS> get-adsroute

Name                             NetId                Protocol   TLS   Address          FingerPrint
----                             -----                --------   ---   -------          -----------
RemoteSystem                     2.2.2.2.1.1          TcpIP            192.168.0.2
```

This is installed by default (see appsettings.json)

Get the remote (PLC) system credentials:

```pwsh
$c = get-credential -UserName tc

PowerShell credential request
Enter your credentials.
Password for user tc: **
```

(Broadcast)Search the remote PLC system by its IP address.

```pwsh
$r = get-adsroute -address 172.30.226.23 -all
$r

Name                             NetId                FingerPrint
----                             -----                -----------
RalfHW1064                       172.19.241.154.1.1   226f1c4889b156f8b94b7d484877f25625ec38e81b5c7e07bd6285f587370a5d
```

Add this system as registered route and test the route.

```pwsh
PS> $r | add-adsroute -Credential $c
PS> $r | Test-AdsRoute -OnlinePorts

Name                 NetId             Port   Latency Result
                                               (ms)
----                 -----             ----   ------- ------
RalfHW1064           172.19.241.154.1… 10     5       Ok
RalfHW1064           172.19.241.154.1… 11     1.8     Ok
RalfHW1064           172.19.241.154.1… 12     1.9     Ok
RalfHW1064           172.19.241.154.1… 30     2       Ok
RalfHW1064           172.19.241.154.1… 131    1.6     Ok
RalfHW1064           172.19.241.154.1… 32828  1.0     Ok
RalfHW1064           172.19.241.154.1… 32829  1.4     Ok
RalfHW1064           172.19.241.154.1… 340    1.6     Ok
RalfHW1064           172.19.241.154.1… 32847  8       Ok
RalfHW1064           172.19.241.154.1… 350    11      Ok
RalfHW1064           172.19.241.154.1… 850    3       Ok
RalfHW1064           172.19.241.154.1… 851    2       Ok
RalfHW1064           172.19.241.154.1… 270    1.4     Ok
RalfHW1064           172.19.241.154.1… 351    59      Ok
```

The Port 851 (a running PLC) should be available with event logging.
The AdsRoute should be listed in the local routes now.

```pwsh
PS> get-adsroute

Name                             NetId                Protocol   TLS   Address          FingerPrint
----                             -----                --------   ---   -------          -----------
RemoteSystem                     2.2.2.2.1.1          TcpIP            192.168.0.2
RalfHW1064                       172.19.241.154.1.1   TcpIP            172.30.226.23    226f1c4889b156f8b94b7d484877f25625ec38e…
```


Using a Test TcEventLogger to communicate to the remote system.
The package **Beckhoff.TwinCAT.TcEventLoggerAdsProxy.Net** >= Version **2.9.3** is necessary for this scenario



```csharp
using TcEventLoggerAdsProxyLib;
using System.Globalization;


namespace AdsProxyTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string targetSystem = "172.19.241.154.1.1";            
            Console.WriteLine("Hello, World!");

            var settings = new TcAmsCommunicationSettings();
            settings.SetTcpLoopbackAddress("127.0.0.1");
            
            var logger = new TcEventLogger();

            logger.MessageSent += (TcMessage message) => Console.WriteLine("Received Message: " + message.GetText(CultureInfo.CurrentCulture.LCID));
            logger.AlarmRaised += (TcAlarm alarm) => Console.WriteLine("Alarm Raised: " + alarm.GetText(CultureInfo.CurrentCulture.LCID));
            logger.AlarmCleared += (TcAlarm alarm, bool bRemove) => Console.WriteLine((bRemove ? "Alarm Cleared and was Confirmed: " : "Alarm Cleared: ") + alarm.GetText(CultureInfo.CurrentCulture.LCID));
            logger.AlarmConfirmed += (TcAlarm alarm, bool bRemove) => Console.WriteLine((bRemove ? "Alarm Confirmed and was Cleared: " : "Alarm Confirmed: ") + alarm.GetText(CultureInfo.CurrentCulture.LCID));

            logger.Connect("172.19.241.154.1.1"); //connect to localhost

            Console.Write("Press 'x' or CTRL+C to quit");
            while (true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.X) break;
            }
        }
    }
}
```