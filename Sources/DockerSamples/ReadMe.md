# Docker Sample

This folder contains 2 different demonstrations to build up a docker scenario independant of any  TwinCAT installation

1. Docker composition with AdsRouterConsole (AdsRouterConsole - AdsServer - AdsCient)
2. Docker composition with AdsOverMqtt (Mosquitto, MQTTBroker - AdsServer - AdsClient)

While the AdsRouterConsole is a simple .NET application, that can run in different scenarious (as standalone console application, as service or embedded in other .NET applictions), the AdsOverMqtt sample uses an existing MQTT Broker service.
Both examples show the parametrization/configuration that is important to run AdsServer/AdsClients within docker.

## Requirements
- Linux Distribution
- .NET SDK 6.0 or later
- Docker

This sample was developed and tested with Ubuntu on WSL (Windows Subsystem for Linux,
[WSL Documentation](https://learn.microsoft.com/en-us/windows/wsl/)) and Ubuntu Docker installation. It should run in other environments also.

## AdsRouterConsole - AdsServer - AdsClient
The docker compose file for this scenario is

```cmd
docker-compose.routerconsole.yml
```

This compose file establishes the following (docker) services:
- AdsRouter
- AdsServer (AmsPort 25000)
- AdsClient (reading the AdsState from the Server cyclically)
- A further powershsell console (pwsh) using the TcXaeMgmt tools (for Administration/Debug purposes)

All these services are running in their own docker containers with their own IPAddress. These 4 containers build up a "virtual" TwinCAT System that acts like one single TwinCAT System, but uses seperated and isolated containers.

![Overview](./RouterConsoleConfiguration.svg)

These containers build up a "virtual" TwinCAT System that acts like one single TwinCAT System via an AdsRouter console application.

For container seperation of AdsClient/AdsServer/Router the TwinCAT internal communication must be reconfigured. Each container is running on its own IPAddress:

| Name | IPAddress |
| - | - |
| router | 192.168.20.2 |
| server | 192.168.20.3 |
| client | 192.168.20.4 |
| pwshclient with TcXaeMgmt Module | 192.168.20.5 |

The (internal) Loopback port of the Router in this example is set to

```
192.168.20.2:48900
```
All device 'internal' communication is done via the Loopback address. Shifting this ensures a seperation from 'standard' TwinCAT systems to operate independently.

For the Router,AdsServer and AdsClient this configuration is brought in by environment variables that are specified in the configuration file:

```env
# Basic ADS-Router config which has to match StaticRoutes on TwinCAT-Host systems
AmsRouter__Name=AdsRouterConsole
AmsRouter__NetId=42.42.42.42.1.1

# Forces the used channel protocol
AmsRouter__ChannelProtocol=Ads

# IP and TCP socket on which the ADS-Router should listen on for incoming client connections
AmsRouter__LoopbackIP=192.168.20.2
AmsRouter__LoopbackPort=48900

# Setting to accept incoming ADS-Client connections from the private container subnetwork
AmsRouter__LoopbackExternalSubnet=192.168.20.0/24
```

see [settings-bridged-network.env](.\settings-bridged-network.env).

E.g in the docker-compose.routerconsole.yml

```yaml
services:
  router:
    env_file: "settings-bridged-network.env"
```
The (docker) container services are implemented in C# projects (ConsoleApplication) located in the subfolders ./AdsServer and ./AdsClient and are implemented that way, that they read these environment variables during application startup and converts them to an 'IConfiguration' instance. The 'PwshClient' docker container acts equally to communicate via the changed loopback ports.

### Running the RouterConsole Sample
Go to the DockerSamples folder:
```bash
cd ./Sources/DockerSamples
```

#### Starting the Sample
```bash
docker compose -f docker-compose.routerconsole.yml up
```

This should create the docker images/containers and start the scenario described above. The interaction between containers should be visible in the console:

```bash
...
pwshclient-1  | Latency       : 00:00:00.0175230
pwshclient-1  | CommandResult : Ok
pwshclient-1  | Method        : Ads
pwshclient-1  | Target        : Local
pwshclient-1  | TargetNetId   : 42.42.42.42.1.1
pwshclient-1  | Port          : 25000
pwshclient-1  | Exception     : 
pwshclient-1  | 
client-1      | State of Server 42.42.42.42.1.1:25000 is: Run
client-1      | State of Server 42.42.42.42.1.1:25000 is: Run
pwshclient-1  | 
pwshclient-1  | Latency       : 00:00:00.0012145
pwshclient-1  | CommandResult : Ok
pwshclient-1  | Method        : Ads
pwshclient-1  | Target        : Local
pwshclient-1  | TargetNetId   : 42.42.42.42.1.1
pwshclient-1  | Port          : 25000
pwshclient-1  | Exception     : 
pwshclient-1  | 
client-1      | State of Server 42.42.42.42.1.1:25000 is: Run
```

This shows that the client and the pwsh client are communication successfully via router container.

Optionally attach to the pwsh console and communicate with router/server
```bash
docker exec -it clientwithrouterconsole-pwshclient-1 pwsh
```
The code that is executed during startup in the pwsh client:
[init.ps1](./PwshClient/init.ps1)

```pwsh
PS> $server = new-tcsession -port 25000
PS> $server | get-adsstate

Target               NetId             Port   State      Latency
                                                          (ms)
------               -----             ----   -----      -------
bb71e66573c7         42.42.42.42.1.1   25000  Run        1.4

PS> $server | Get-TcSymbol -recurse

InstancePath    Category  DataType    Size Static Persistant IG IO
------------    --------  --------    ---- ------ ---------- -- --
Globals         Struct                0    False  False      0  0
Globals.bool1   Primitive BOOL        1    False  False      2  1000
Globals.int1    Primitive INT         2    False  False      2  1001
Globals.dint1   Primitive DINT        4    False  False      2  1003
Globals.real1   Primitive REAL        4    False  False      2  1007
Globals.lreal1  Primitive LREAL       8    False  False      2  100B
Globals.string1 String    WSTRING(80) 162  False  False      2  1013
```

#### Stopping the RouterConsole Sample
Ctrl-C and
```bash
docker compose -f docker-compose.routerconsole.yml down
```

#### Building and running the RouterConsole containers manually
This is only for reference documentation here. Use the docker compose up/down above for standard needs.

##### Build
```bash
docker build -t adsrouter --no-cache --target=final --file ./AdsRouterConsole/Dockerfile .
docker build -t adsserver --no-cache --target=final --file ./AdsServer/Dockerfile .
docker build -t adsclient --no-cache --target=final --file ./AdsClient/Dockerfile .
docker build -t pwshclient --no-cache --target=final --file ./PwshClient/Dockerfile .
```

##### Run Interactively:
```bash
docker run -it --rm --name router --env-file="settings-bridged-network.env" --network bridge adsrouter
docker run -it --rm --name server --env-file="settings-bridged-network.env" --network bridge adsserver
docker run -it --rm --name client --env-file="settings-bridged-network.env" --network bridge adsclient
docker run -it --rm --name adsClient --env-file="settings-bridged-network.env" --network bridge pwshClient
```

##### Run Non-interactive:
```bash
docker run -d --rm --name router --env-file="settings-bridged-network.env" --network bridge adsrouter
docker run -d --rm --name server --env-file="settings-bridged-network.env" --network bridge adsserver
docker run -d --rm --name client --env-file="settings-bridged-network.env" --network bridge adsclient
docker run -d --rm --name pwshclient --env-file="settings-bridged-network.env" --network bridge pwshclient
```

##### Connect to the containers shell:
```bash
docker exec -it router sh
```

## MQTTBroker - AdsServer - AdsClient
As second scenario a MQTT Broker is used to replace the 'TwinCAT Router' or 'AdsRouterConsole'. The structure is the same as the docker compose example for the AdsRouterConsole in the previous chapter. Instead of processing the routing in the AdsRouterConsole container, a MQTT Broker (Mosquitto) is used.

![Overview](./MQTTConfiguration.svg)

These containers build up a "virtual" TwinCAT System that acts like one single TwinCAT System via a MQTT Broker.

Each container is running on its own
IPAddress (like in the docker AdsRouter example):

| Name | IPAddress |
| - | - |
| mosqitto | 192.168.20.2 |
| adsserver | 192.168.20.3 |
| adsclient | 192.168.20.4 |
| pwshclient with TcXaeMgmt Module | 192.168.20.5 |

The configuration for the MQTT communication is here brought in by the environment variables defined in the file config-mqtt.env

[config-mqtt.env](./config-mqtt.env)

E.g in the docker-compose.mqtt.yml
```yaml
services:
  router:
    env_file: ""config-mqtt.env""
```

which maily sets the used local AmsNetId and the Address/Port of the MQTT broker.
```env
# Sets the AdsOverMqtt ChannelProtocol for default
AmsRouter__ChannelProtocol=AdsOverMqtt

# Basic ADS-Router config which has to match StaticRoutes on TwinCAT-Host systems
AmsRouter__Name=MqttRouter
AmsRouter__NetId=42.42.42.42.1.1

AmsRouter__Mqtt__0__Address=192.168.20.2
AmsRouter__Mqtt__0__Port=1883
AmsRouter__Mqtt__0__Topic=VirtualAmsNetwork1
```

The (docker) container services are implemented in C# projects (ConsoleApplication) located in the subfolders ./AdsServer and ./AdsClient and are implemented that way, that they read these environment variables during application startup and converts them to an 'IConfiguration' instance. The 'PwshClient' docker container acts equally to communicate via AdsOverMqtt.

### Running the Sample
The behaviour should be the same as described in the RouterConsole scenario.
Just a different docker compose file is used!

Go to the DockerSamples folder:
```bash
cd ./Sources/DockerSamples
```

#### Starting the Sample
```bash
docker compose -f docker-compose.mqtt.yml up
```

#### Stopping the Sample
```bash
docker compose -f docker-compose.mqtt.yml down
```

## Some hints to run the Docker Examples in Domain Network
Developing and debugging these docker containers need some furhter configuration when done in an Azure DevOps system like it is done on the Beckhoff site. Here is a collection of some 'Hints' and 'HowTos' to manage such an environment.

#### Using 'On-premise' Azure Devops feeds for builds
The environment variable
```bash
AZDEVOPS_ACCESS_TOKEN
```
must contain a valid PAT to access the DevOps feed.

Here we define a variable to access a Beckhoff DevOps Feed 'TcBase'.
To access the feed in the docker build phase we need a set environment variable like this:

```bash
export NuGetPackageSourceCredentials_TcBase="Username=DevOpsBuild@beckhoff.com;Password=$AZDEVOPS_ACCESS_TOKEN;ValidAuthenticationTypes=Basic"
```
The docker build restore step can automatically use the variable to access 'TcBase' as defined.
[MoreInfo](https://www.davidpuplava.com/coding-craft/how-to-use-private-nuget-feed-with-docker-build)

To access the internal 'TcBase' Feed during docker build:

```bash
docker build -t adsrouter --no-cache --target=final --file ./AdsRouterConsole/Dockerfile --build-arg NuGetPackageSourceCredentials_TcBase=$NuGetPackageSourceCredentials_TcBase .
docker build -t adsserver --no-cache --target=final --file ./AdsServer/Dockerfile --build-arg NuGetPackageSourceCredentials_TcBase=$NuGetPackageSourceCredentials_TcBase .
docker build -t adsclient --no-cache --target=final --file ./AdsClient/Dockerfile --build-arg NuGetPackageSourceCredentials_TcBase=$NuGetPackageSourceCredentials_TcBase .
docker build -t pwshclient --no-cache --target=final --file ./PwshClient/Dockerfile --build-arg NuGetPackageSourceCredentials_TcBase=$NuGetPackageSourceCredentials_TcBase --progress=plain .
```
#### Solve docker IPAddress conflicts with Domain IPRange.
When using docker inside the domain network the docker network might conflict with the standard Domain IPs.
To fix this issue you need to create a file at /etc/docker/daemon.json:

```bash
/etc/docker/daemon.json
```
```json
{
       "dns":["172.17.0.3", "172.17.0.4"],
       "dns-search"    :["beckhoff.com"],
       "bip":"192.168.220.1/22",
       "default-address-pools":[
            {"base":"192.168.224.0/20","size":26}
       ],
       "shutdown-timeout": 3600
}
```

After changing this file docker should be restarted:

```bash
sudo ip link del docker0
sudo systemctl restart docker
 ```
