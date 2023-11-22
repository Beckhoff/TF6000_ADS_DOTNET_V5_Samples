Along with the deployment of the application where the TcpRouter is implemented, a valid Router / ADS configuration must be placed to specify
the Local Net ID, the name and the default port of the Router system.

The preferred way to configure the system is with standard Configuration providers, which are part of the
.NET Core / ASP .NET Core infrastructure.

For more information how to implement and deploy your own Router please have a look at:
[RouterSamples](https://github.com/Beckhoff/TF6000_ADS_DOTNET_V5_Samples/tree/main/Sources/RouterSamples)
[DockerSamples](https://github.com/Beckhoff/TF6000_ADS_DOTNET_V5_Samples/tree/main/Sources/DockerSamples)
[Microsoft]https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1

This enables common options for application configuration that can be used 'out-of-the-box':

- Via the file appsettings.json
- With the StaticRoutesConfigurationProvider (StaticRoutes.xml)
- Using Environment Variables.
- Command line arguments
- etc.

The configuration has to be loaded during application startup and is placed into the **'TwinCAT.Ads.TcpRouter.AmsTcpIpRouter'** class via constructor dependency injection and
must contain the following information:

- The name of the local System (usually the Computer or Hostname)
- The Local AmsNetId of the local system as Unique Address in the network
- Optionally the used TcpPort (48898 or 0xBF02 by default)
- The static routes in the 'RemoteConnections' list.
- Logging configuration.

Actually the configuration is not reloaded during the runtime of the **'TwinCAT.Ads.TcpRouter.AmsTcpIpRouter'** class.
Please be aware that the "Backroute" from the Remote system linking to the local system (via AmsNetId) is necessary also to get functional routes.

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
      "Default": "Information",
      "System": "Information",
      "Microsoft": "Information"
    },
    "Console": {
      "IncludeScopes": true
    }
  }
}
```

Alternatively a "StaticRoutes.Xml" Xml File can configure the system equally. Don't forget to add the **'StaticRoutesXmlConfigurationProvider'** to the Host configuration
during startup (see FirstSteps below).

An example of the local "StaticRoutes.xml" is given here:

```xml
<?xml version="1.0" encoding="utf-8"?>
<TcConfig xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="C:\TwinCAT3\Config\TcConfig.xsd">
  <Local>
      <Name>MyLocalSystem</Name>
      <NetId>192.168.1.20.1.1</NetId> <!-- Local NetId -->
      <TcpPort>48898</TcpPort> <!-- Default TcpPort -->
  </Local>
  <RemoteConnections>
    <Route>
      <Name>RemoteSystem1</Name>
      <Address>RemoteSytem</Address> <!-- HostName -->
      <!--<Address>192.168.1.21</Address>  --> <!--IPAddress -->
      <NetId>192.168.1.21.1.1</NetId>
      <Type>TCP_IP</Type>
    </Route>
    <Route>
      <Name>RemoteSystem2</Name>
      <Address>192.168.1.22</Address> <!-- IPAddress -->
      <!--<Address>RemoteSystem2</Address>  --> <!--HostName -->
      <NetId>192.168.1.21.1.1</NetId>
      <Type>TCP_IP</Type>
    </Route>
  </RemoteConnections>
</TcConfig>
```

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
PS> $env:AmsRouter:Logging:LogLevel:Default = 'Information'
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
AmsRouter:Logging:LogLevel:Default    Information
```