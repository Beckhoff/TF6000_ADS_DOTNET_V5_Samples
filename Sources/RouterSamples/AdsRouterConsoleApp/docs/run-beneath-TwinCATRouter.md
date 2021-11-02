# Run the AdsRouterConsoleApp together with the standard installed TwinCAT Router
One sophisticated scenario is to run the AdsRouterConsoleApp beneath the standard TwinCAT Router.
The result will be two different (virtual) independent ADS networks.

Because the standard Router 'blocks' the IPLoopback for its own loopback communication,
the AdsRouterConsoleApp must be configured to different Addresses and ports.

# Example for a configuration

Local IPAddress:
192.168.2.1

TwinCAT Router AmsNetId: 1.1.1.1.1.1 (internal channel 127.0.0.1:48998, external 192.168.2.1: 48998)
AdsRouterConsole AmsNetId: 2.2.2.2.1.1 (internal channel 192.168.2.1: 48900, external 192.168.2.1: 48901)

AdsClients and AdsServers that can communicate via the TwinCAT Router must run on this machine (IPAddress 127.0.0.1)
AdsClients and AdsServers that can communicate via the AdsRouterConsole are in the Network 192.168.2.0/24

The LoopbackExternalSubnet / LoopbackExternals setting can be used alternatively on AdsRouterConsole application.

## TwinCAT Router
| Name          | Value             | Description   | Configurable |
| ----          | ---               | ---           | --- |
| AmsNetId      | 1.1.1.1.1.1       | AmsNetId of the TwinCAT Router | true |
| LoopbackIP    | 127.0.0.1         | LoopbackIP of the TwinCAT Router | false |
| LoopbackPort  | 48998 (0xBF02)    | Used TCP/IP port for the 'internal' communication channel | false |
| ExternalPort  | 48998 (0xBF02)    | Used TCP/IP Port for external ADS communication | false |
| LoopbackExternalSubnet  | 127.0.0.1/32 | Loopback access is only allowed for Source IP 127.0.0.1 | false |
| LoopbackExternals  | 127.0.0.1 | Loopback access is only allowed for Source IP 127.0.0.1 | false |

## TcpIpRouter Component / AdsRouterConsole application

| Name          | Value             | Description   | Configurable |
| ----          | ---               | ---           | --- |
| AmsNetId      | 2.2.2.2.1.1       | AmsNetId of the AdsRouterConsole/TcpIpRouter | true |
| LoopbackIP    | 192.168.2.1       | (external) IPAddress that is allowed to use the 'internal' ADS communication port | true |
| LoopbackPort  | 48900             | Used TCP/IP port for the 'internal' communication channel | true |
true |
| ExternalPort  | 48901 | Used TCP/IP Port for ADS communication | false |
| LoopbackExternalSubnet  | 192.168.2.0/24 | internal access only allowed for the subnet 192.168.2.0/24 | 
| LoopbackExternals  | 192.168.2.3, 192.168.2.4 | or alternativly internal access is only allowed for this list of IPAddresses | true |

### Config.Json for the AdsRouterConsole

```json
{
  "AmsRouter": {
    "Name": "AdsRouterConsole",
    "NetId": "2.2.2.2.1.1",
    "TcpPort": 48900,
    "LoopbackIP": "192.168.2.1",
    "LoopbackPort": 48901,
    
    "LoopbackExternalSubnet": "192.168.2.0/24",
    // Or alternatively
    // "LoopbackExternals": [
    //         { "IP": "192.168.2.3" },
    //         { "IP": "192.168.2.4" },
    //     ]
    }
}
```

### Instantiation with parameters of the AmsTcpIpRouter class
```csharp
AmsNetId localNetId = new AmsNetId("2.2.2.2.1.1");
IPAddress loopbackIP = IPAddress.Parse("192.168.2.1");
int loopbackPort = 48900;
int externalPort = 48901;
IPNetwork loopbackExternalSubnet = new IPNetwork(“192.168.2.0/24”);
AmsTcpIpRouter _router = new AmsTcpIpRouter(localNetId, loopbackPort, loopbackIP, externalPort, loopbackExternalSubnet, _logger);
```