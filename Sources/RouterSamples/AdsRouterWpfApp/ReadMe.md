# AdsRouterWpfApp
This sample shows how to establish an AmsTcpIpRouter within an WPF App

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
MYSYSTEM                         1.1.1.1.1.1          TcpIP            192.168.0.1
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
