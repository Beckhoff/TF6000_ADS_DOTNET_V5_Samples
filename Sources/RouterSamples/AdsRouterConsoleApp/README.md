## Description of the **AdsRouterConsoleApp** Sample application
The project contains a lean TCP ADS Router as binary. It is a simple console application
'ready-to-run' (with included simple AdsServers Port 1 and Port 10000).

It can be used in scenarios where no standard TwinCAT router is established or available and is running in UserMode only (no realtime characteristics) and contains no further functionality than distributing the ADS Frames (e.g. no Port 10000, no ADS Secure). It is just used to route ADS frames locally between AdsServers and give some basic support of Route management and browsing Remote Systems.
and to/from remote ADS devices.

## Requirements
- No other System allocating the same port (e.g. a regular TwinCAT installation) 

This sample works across all platforms that are supported by the .net core runtime (windows, linux, macOS). This is called a "portable" or "framework dependant" deployment.
If an *.exe really is needed, please consider self-contained deployments.

Along with the deployment of the application where the TcpRouter is implemented, a valid Router / ADS configuration must be placed to specify
the Local Net ID, the name and the default port of the Router system.

The preferred way to configure the system is with standard Configuration providers, which are part of the
.NET Core / ASP .NET Core infrastructure.

See further information:
https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1

This enables common options for application configuration that can be used 'out-of-the-box':

- Via the file appsettings.json
- With the StaticRoutesConfigurationProvider (StaticRoutes.xml)
- Using Environment Variables.
- Command line arguments
- etc.

The configuration has to be loaded during application startup and is placed into the **'Beckhoff.TwinCAT.Ads.AdsRouterConsole'** application via dependency injection and
must contain the following information:
- The name of the local System (usually the Computer or Hostname)
- The Local AmsNetId of the local system as Unique Address in the network
- Optionally the used TcpPort (48898 or 0xBF02 by default)
- The static routes in the 'RemoteConnections' list.
- Logging configuration.

Actually the configuration is not reloaded during the runtime of the **'Beckhoff.TwinCAT.Ads.AdsRouterConsole'** application.
Please be aware that the "Backroute" from the Remote system linking to the local system (via AmsNetId) is necessary also to get functional routes.

### Json
Example for a valid 'appSettings.json' file (please change the Addresses for your network/systems.)

```json
{
  "AmsRouter": {
    "Name": "MyLocalSystem",
    "NetId": "192.168.1.20.1.1",
    "TcpPort": 48898,
    "RemoteConnections": [
      {
        "Name": "RemoteSystem1",
        "Address": "RemoteSystem1",
        "NetId": "192.168.1.21.1.1",
        "Type": "TCP_IP"
      },
      {
        "Name": "RemoteSystem2",
        "Address": "192.168.1.22",
        "NetId": "192.168.1.22.1.1",
        "Type": "TCP_IP"
      },
    ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```
### StaticRoutes.xml
Alternatively a "StaticRoutes.Xml" Xml File can configure the system in the same manner.

An example of the local "StaticRoutes.xml" is given here:

```xml
<?xml version="1.0" encoding="utf-8"?>
<TcConfig xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="C:\TwinCAT3\Config\TcConfig.xsd">
  <Local>
      <Name>MyLocalSystem</Name>
      <NetId>192.168.1.22.1.1</NetId> <!-- Local NetId -->
  </Local>
  <RemoteConnections>
    <Route>
      <Name>MyRemoteSystem</Name>
      <Address>RemoteSytem</Address> <!-- HostName -->
      <!--<Address>192.168.1.21</Address>  --> <!--IPAddress -->
      <NetId>192.168.1.21.1.1</NetId>
      <Type>TCP_IP</Type>
    </Route>
  </RemoteConnections>
</TcConfig>
```

This file must be edited to configure the ADS Router. It is not reloaded during the runtime of the **'Beckhoff.TwinCAT.Ads.AdsRouterConsole'** application.

### Configuration by Environment Variables
Alternatively, the configuration can also be set via Environment variables.

```Powershell
PS> $env:AmsRouter:Name = 'MyLocalSystem'
PS> $env:AmsRouter:NetId = '192.168.1.20.1.1'
PS> $env:AmsRouter:TcpPort = 48898
PS> $env:AmsRouter:RemoteConnections:0:Name = 'RemoteSystem1'
PS> $env:AmsRouter:RemoteConnections:0:Address = 'RemoteSystem1'
PS> $env:AmsRouter:RemoteConnections:0:NetId = '192.168.1.21.1.1'
PS> $env:AmsRouter:RemoteConnections:1:Name = 'RemoteSystem2'
PS> $env:AmsRouter:RemoteConnections:1:Address = '192.168.1.22'
PS> $env:AmsRouter:RemoteConnections:1:NetId = '192.168.1.22.1.1'
PS> $env:AmsRouter:Logging:LogLevel:Default = 'Warning'
```

```Powershell
PS> dir env: | where Name -like AmsRouter* | format-table -AutoSize

Name                                  Value
----                                  -----
AmsRouter:Name                        MyLocalSystem
AmsRouter:NetId                       192.168.1.20.1.1
AmsRouter:TcpPort                     48898
AmsRouter:RemoteConnections:0:Name    RemoteSystem1
AmsRouter:RemoteConnections:0:Address RemoteSystem1
AmsRouter:RemoteConnections:0:NetId   192.168.1.21.1.1
AmsRouter:RemoteConnections:1:Name    RemoteSystem2
AmsRouter:RemoteConnections:1:Address 192.168.1.22
AmsRouter:RemoteConnections:1:NetId   192.168.1.22.1.1
AmsRouter:Logging:LogLevel:Default    Warning
```

## First Steps

### Running as .NET Core or .NET Standard application
```Shell
dotnet run .\TwinCAT.Ads.AdsRouterConsole.dll
```

### Running as Full Framework Application
```Shell
TwinCAT.Ads.AdsRouterConsole.exe
```

### Testing
The Sample Application instantiates AdsServers beneath the Routers:
1. Router AdsServer (Port 1)
2. SystemService AdsServer (Port 10000)

Therefore, this application can be tested very easy with the 'TcXaeMgmt' Powershell Module. Just install this module into the local Powershell

[Installation](https://www.powershellgallery.com/packages/TcXaeMgmt)

[Documentation](https://infosys.beckhoff.com/english.php?content=../content/1033/tc3_ads_ps_tcxaemgmt/3972231819.html&id=8731138690123386389)

Installing the TcXaeMgmt Module
```pwsh
PS> install-module TcXaeMgmt
PS> import-module TcXaeMgmt
```

Determine the local AmsNetId
```pwsh
PS> Get-AdsRoute -local

Name                             NetId                Protocol   TLS   Address          FingerPrint
----                             -----                --------   ---   -------          -----------
MYSYSTEM                          1.1.1.1.1.1          TcpIP            192.168.0.1
```

Testing the Local AdsServers (Router and SystemService)
```pwsh

PS> Test-AdsRoute -port 1
Name                 Address           Port   Latency Result
                                               (ms)
----                 -------           ----   ------- ------
MYSYSTEM              1.1.1.1.1.1       1      4       Ok

PS> Test-AdsRoute -port 10000

Name                 Address           Port   Latency Result
                                               (ms)
----                 -------           ----   ------- ------
MYSYSTEM              1.1.1.1.1.1       10000  0.9     Ok
```

Getting remote routes of the local system 
```pwsh
PS> Get-AdsRoute

Name                             NetId                Protocol   TLS   Address          FingerPrint
----                             -----                --------   ---   -------          -----------
CodedRemote                      3.3.3.3.1.1          TcpIP
```

Broadcast Search
```pwsh
PS> Get-AdsRoute -all

Name                             NetId                Protocol   TLS   Address          FingerPrint   TcVersion    RTSystem
----                             -----                --------   ---   -------          -----------   ---------    --------
MYSYSTEM                         1.1.1.1.1.1          TcpIP            192.168.0.1                    [UNKNOWN]    [UNKNOWN]
CX_11111                         1.1.1.1.1.2          TcpIP      X     192.168.0.2      478c762e...   3.1.4025     TcBSD 13.2
CX_11112                         1.1.1.1.1.3          TcpIP      X     192.168.0.3                    3.1.4022     CE7.0
CX_11113                         1.1.1.1.1.4          TcpIP      X     192.168.0.4      ab35ff7f...   3.1.4024     Win10 (21H2)
CX_11114                         1.1.1.1.1.5          TcpIP      X     192.168.0.5      4528dc85...   3.1.4024     Win10 (22H2)
```

Adding and Removing Routes (help)

```pwsh
PS> get-help Add-AdsRoute -examples
PS> get-help Remove-AdsRoute -examples
```



## Further documentation
The actual version of the documentation is available in the Beckhoff Infosys.
[Beckhoff Information System](https://infosys.beckhoff.com/index.php?content=../content/1033/tc3_ads.net/index.html&id=207622008965200265)