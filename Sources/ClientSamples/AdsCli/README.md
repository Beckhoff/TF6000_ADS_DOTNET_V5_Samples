# ADS Command Line Interface (CLI) Client - Sample

This project is intended as sample and should demonstarte the usage of the `Beckhoff.TwinCAT.Ads` package for the purpose of a simple ADS client application.

Furthermore, it includes a `Dockerfile` to demonstarte the packaging of the client application as a container image and explains the execution of the client container in combination with the `AdsRouterConsoleApp` sample.

## Usage as Container

To build the container image run `docker build -t ads-cli-client:latest .` from within the root path of this sample project.

Afterwards `docker image ls` should list the `ads-cli-client` image in the local repository.

```
$ docker image ls
REPOSITORY                           TAG       IMAGE ID       CREATED         SIZE
ads-cli-client                       latest    5f8351ca68d3   9 minutes ago   191MB
...
```

The sample application does not contain any ADS routing service.
Therefore, an available ADS router service can be specified to the `ads-cli-client` application via the envrionment variables `AmsConfiguration:LoopbackAddress` and `AmsConfiguration:LoopbackPort`.

Per default both variables are set to:

```
AmsConfiguration:LoopbackAddress=127.0.0.1
AmsConfiguration:LoopbackPort=48898
```

To point the `ads-cli-client` application to an `AdsRouterConsoleApp` instance you can pass suitable values via the `--env <key>=<value>` option of the `docker run` command.
For instance:

```
docker run -it --rm \
--env "AmsConfiguration:LoopbackAddress=172.17.0.2" \ 
--env "AmsConfiguration:LoopbackPort=48900" \
ads-cli-client -v 5.76.88.215.1.1 'INT' 'MAIN.counter' '16'
```