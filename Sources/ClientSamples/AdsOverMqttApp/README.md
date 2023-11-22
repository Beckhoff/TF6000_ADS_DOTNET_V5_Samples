## **AdsOverMqttApp** Sample application
This sample describes an AdsClient console application that runs transparently via MQTT. The AdsClient doesn't need a running TwinCAT Router, but instead it connects to a preconfigured MQTT Broker. Communication is done via AdsOverMQTT.
This demonstrates a scenario, where no full TwinCAT installation is available or not practical, e.g:

- Processing a AdsClient/AdsServer application in a docker container
- Small footprint devices without further TwinCAT overhead (but access to a MQTT broker)

## Requirements

- .NET 6.0 SDK
- A Mqtt Broker that runs the AdsOverMqtt protocol
- For this demo a device running a TwinCAT PLC and a counter symbol

  ```Iec11131
  PROGRAM MAIN
  VAR
    i : INT;
  END_VAR

  i := i+1;
  ```

- A running MQTT broker that is alreay bound to the TwinCAT device by AdsOverMQTT ([InfoSys Documentation](https://infosys.beckhoff.com/content/1033/tc3_ads_over_mqtt/index.html?id=120186874503837909))


## Configuration
This example is using a preconfigured Beckhoff TwinCAT Cloud Instance with MQTT Broker (here with TLS CA Security). This must be replaced and configured by your own Broker and Device.

![Image](.\Overview.svg)

The configuration is done in this example by

- [ApplicationSettings appSettings.json](./src/appSettings.json) or 
- [Environment Variables settings.env](./src/settings.env) (disabled in code)

For more information how to setup your MQTT broker and how the parametrization is done have a look at:

- [InfoSys: ADSOverMqtt](https://infosys.beckhoff.com/english.php?content=../content/1033/tc3_grundlagen/4320983179.html&id=)
- at the Beckhoff.TwinCAT.ConfigurationProvider Package API description
- [GitHub: AdsOverMqtt](https://github.com/Beckhoff/ADS-over-MQTT_Samples)

## Sample Description
The sample uses standard IConfiguration and ILoggerFactory infrastructure which is provided by a HostBuilder. This is a best-practice in .NET applications and ensures that configurations can be injected to the main application in different ways. The sample code in [Program.cs](.\src\Program.cs) demonstrates the following options from different configuration sources:
  
- Application Settings
- Environment Variables
- or TwinCAT StaticRoutes.xml file

```json
{
  "TargetNetId": "3.79.104.236.1.1",
  "AmsRouter": {
    "Name": "MqttRouter",
    "NetId": "42.42.42.42.1.1",
    "ChannelProtocol": "All",
    "Mqtt": [
      {
        "NoRetain": false,
        "Unidirectional": false,
        "Port": 8883,
        "Address": "ba-0f8cfe2680560cffb.eu-central-1.demo.beckhoff-cloud-instances.com",
        "Topic": "VirtualAmsNetwork1",
        "Tls": {
          "IgnoreCn": false,
          "CA": "C:\\Program Files (x86)\\Beckhoff\\TwinCAT\\3.1\\Target\\Certificates\\BA-0f8cfe2680560cffb\\intermediateCA.pem",
          "CERT": "C:\\Program Files (x86)\\Beckhoff\\TwinCAT\\3.1\\Target\\Certificates\\BA-0f8cfe2680560cffb\\MyDevice.pem",
          "KEY": "C:\\Program Files (x86)\\Beckhoff\\TwinCAT\\3.1\\Target\\Certificates\\BA-0f8cfe2680560cffb\\MyDevice.key",
          "Version": "tlsv1.2"
        }
      }
    ]
  }
}
```
Beneath the 'Mqtt' section - which defines the MQTT Broker Address and Security settings, the following entries are important:

|Name | Value | Description |
| - | - | - |
| TargetNetId | 3.79.104.236.1.1 | The NetId of the remote/target device used by this application |
| AmsRouter:Name | MqttRouter | Name of the local device (optional) |
| AmsRouter:NetId | 42.42.42.42.1.1 | The Local AmsNetId |
| AmsRouter:ChannelProtocol | All | Channel protocol to use<br>All: ADS and Fallback AdsOverMqtt (Default)<br>Ads: ADS only<br>AdsOverMqtt: Mqtt only |

The sample reads the MAIN.i symbol in a loop. See the *ReadValueAsync* access in the source file 'HostedService.cs'

## Running the sample
Make sure, no TwinCAT Router is running locally. On TwinCAT Systems it could be stopped with:
```pwsh
PS> stop-service TcSysSrv
```
or
```cmd
net stop TcSysSrv
```

If the parametrization/configuration is changed and valid, run your MQTT Broker, the TwinCAT Target system and then start this test application with:

```cmd
cd .\src
dotnet run --project .\AdsOverMqttApp.csproj
``` 

The output should show something like this:

```cmd
Address:3.79.104.236.1.1:851 Symbol: MAIN.i Value: 1218
Address:3.79.104.236.1.1:851 Symbol: MAIN.i Value: 1273
Address:3.79.104.236.1.1:851 Symbol: MAIN.i Value: 1328
Address:3.79.104.236.1.1:851 Symbol: MAIN.i Value: 1382
Address:3.79.104.236.1.1:851 Symbol: MAIN.i Value: 1437
...
```

This shows that the MQTT channel is selected and the MAIN.i symbol could be read successfully.
