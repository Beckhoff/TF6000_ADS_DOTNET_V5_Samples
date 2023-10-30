# Package and deploy the AdsRouterConsoleApp as a Container Image

## Build the image

Change directory to the project root folder (where the *.sln files are located).

From within the project root run the `docker build` process with appropriated arguments.

For instance, the following command builds an `ads-router-console` image based on the provided `Dockerfile`.

```sh
docker build -t ads-router-console --target=final --file Dockerfile .
```

**Don't miss the `.` at the end of the `docker build` command.**
The `.` sets the current directoy (which should be the root of the repository) as the build context which serves as root for relative paths during the build process.

Afterwards `docker image ls` should list the build `ads-router-console` image.

## Run the image as a container on the container-host network

The `AdsRouterConsoleApp` expects AMS route configurations to be passed as envrionment variables.

Only environment variables starting with `ENV_` will be read by the application.
In the following you find a minimal example set of sutiable variables.

```sh
# Basic ADS-Router config which has to match StaticRoutes on TwinCAT-Host systems
ENV_AmsRouter__Name=AdsRouterConsole
ENV_AmsRouter__NetId=55.123.98.42.1.1

# Indexed List of remote connections to TwinCAT Hosts
# First TwinCAT-Host
ENV_AmsRouter__RemoteConnections__0__Name=TwinCAT-Host
ENV_AmsRouter__RemoteConnections__0__Address=192.168.178.72
ENV_AmsRouter__RemoteConnections__0__NetId=5.29.122.232.1.1

# Another sample TwinCAT-Host
ENV_AmsRouter__RemoteConnections__1__Name=Another-TwinCAT-Host
ENV_AmsRouter__RemoteConnections__1__Address=192.168.178.74
ENV_AmsRouter__RemoteConnections__1__NetId=19.58.12.202.1.1

# Verbose log output
ENV_Logging__LogLevel__Default=Debug
```

To pass environment variables to a container instance you can use the [-e, --env or --env-file option](https://docs.docker.com/engine/reference/commandline/run/#set-environment-variables--e---env---env-file)

In the following example, the set of environment variables is passed via `--env-file` as argument of the `docker run` command.

```sh
docker run \
-it \
--rm \
--name adsrouter \
--env-file="src/settings-host-network.env" \
--network host \
ads-router-console
```

## Run the image as a container inside a bridged container network

If you leave out the `--network host` option in the `docker run` command, docker assigns the container to the default bridge network (even though the [default bridge network is is considered as a legacy detail of Docker](https://docs.docker.com/network/bridge/#use-the-default-bridge-network)).
As a result, Docker configures a bridge device and assigns a private IP address to that bridge which is usually referred to as `docker0`.

```sh
$ ip addr show dev docker0
3: docker0: <NO-CARRIER,BROADCAST,MULTICAST,UP> mtu 1500 qdisc noqueue state DOWN group default 
    link/ether 02:42:11:29:6a:bc brd ff:ff:ff:ff:ff:ff
    inet 172.18.0.1/16 brd 172.18.255.255 scope global docker0
       valid_lft forever preferred_lft forever
    inet6 fe80::42:11ff:fe29:6abc/64 scope link 
       valid_lft forever preferred_lft forever
```

In bridge mode, each started container owns a (virtual) network stack which gets configured by docker during the container start.
During the configuration docker assigns each container a IP address in the subnet of the `docker0` bridge.
Likewise, the previously assigned IP address of the `docker0` bridge is set as default route within each container.
Finally, docker utilizes the `iptables` on the container-host to apply network address translation as well as port forwarding between the container network, the `docker0` bridge and the container host network.

Running the `AdsRouterConsole` inside a container within a bridged network requires the assignmend of a `LoopbackIP` and a `LoopbackPort`.
Furthermore, the `AdsRouterConsole` needs information about IP addresses of ADS-Client applications, deployed as seperated containers in the private subnet of the bridged network.
All required settings should be passed as envrionment variables.
Again, only environment variables starting with `ENV_` will be read by the application.
In the following you find a minimal example set of sutiable variables for a bridged network setup:

```sh
# Basic ADS-Router config which has to match StaticRoutes on TwinCAT-Host systems
ENV_AmsRouter__Name=AdsRouterConsole
ENV_AmsRouter__NetId=55.123.98.42.1.1

# IP and TCP socket on which the ADS-Router should listen on for incoming client connections
ENV_AmsRouter__LoopbackIP=172.17.0.2
ENV_AmsRouter__LoopbackPort=48900

# Setting to accept incoming ADS-Client connections from the private container subnetwork
ENV_AmsRouter__LoopbackExternalSubnet=172.17.0.0/16

# Indexed List of remote connections to TwinCAT Hosts
# First TwinCAT-Host
ENV_AmsRouter__RemoteConnections__0__Name=TwinCAT-Host
ENV_AmsRouter__RemoteConnections__0__Address=192.168.178.72
ENV_AmsRouter__RemoteConnections__0__NetId=5.29.122.232.1.1

# Another sample TwinCAT-Host
ENV_AmsRouter__RemoteConnections__1__Name=Another-TwinCAT-Host
ENV_AmsRouter__RemoteConnections__1__Address=192.168.178.74
ENV_AmsRouter__RemoteConnections__1__NetId=19.58.12.202.1.1

# Verbose log output
ENV_Logging__LogLevel__Default=Debug
```

To pass environment variables to a container instance you can use the [-e, --env or --env-file option](https://docs.docker.com/engine/reference/commandline/run/#set-environment-variables--e---env---env-file)

In the following example, a set of environment variables is passed via `--env-file` as argument of the `docker run` command.
In addition, we can pass the `--ip` option to match the `ENV_AmsRouter__LoopbackIP=172.17.0.2` setting.
Likewise the `-p` option is used to map the `ENV_AmsRouter__LoopbackPort=48900` port of the container to the host port `48898`.

```sh
docker run \
-it \
--rm \
--name adsrouter \
--env-file="src/settings-bridged-network.env" \
--ip 172.17.0.2 \
-p 48898:48900 \
ads-router-console
```
