## Description of the **AdsRouterConsoleApp** Sample application
The package **'Beckhoff.TwinCAT.Ads.AdsRouterConsole'** contains a lean TCP ADS Router as binary. It is a simple console application
'ready-to-run'.

It can be used in scenarios where no standard TwinCAT router is established or available and is running in UserMode only (no realtime characteristics) and contains no further functionality than distributing the ADS Frames (e.g. no Port 10000, no ADS Secure). It is just used to route ADS frames locally between AdsServers 
and to/from remote ADS devices.

## Requirements
- **.NET 5.0**, **.NET Core 3.1**, **.NET Framework 4.61** or **.NET Standard 2.0** compatible SDK or later
- No other System allocating the same port (e.g. a regular TwinCAT installation) 

## Installation

```Shell
dotnet add package Beckhoff.TwinCAT.Ads.AdsRouterConsole
```

This will install the **AdsRouterConsole** application.
For the .NET FullFramework the package contains the **TwinCAT.Ads.AdsRouterConsole.exe** which acts as Console application directly.
For other platforms (.NET Core and .NET Standard) it contains the **TwinCAT.Ads.AdsRouterConsole.dll** which is indirectly started by the .NET CLI.

NET Core applications are supposed to be .dllfiles. OutputType set to Exe in this case means "executable" and does everything necessary to ensure that the output is runnable (entry point from Main() method,
The resulting dll file is meant to be run using:

```shell
dotnet Beckhoff.TwinCAT.Ads.AdsRouterConsole.dll 
```

This dll file works across all platforms that are supported by the .net core runtime (windows, linux, macOS). This is called a "portable" or "framework dependant" deployment.
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
## Further documentation
The actual version of the documentation is available in the Beckhoff Infosys.
[Beckhoff Information System](https://infosys.beckhoff.com/index.php?content=../content/1033/tc3_ads.net/index.html&id=207622008965200265)