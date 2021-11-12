# Using of the CustomSessionProvider Sample
A custom Session provider extends the Session handling of the Beckhoff.TwinCAT.Ads package by your custom protocol.
The provider is integrated in the TwinCAT System as MEF component, so just compile the Sample and copy the resulting dll beneath the TwinCAT.Ads.dll (which acts as MEF controller) or inside the TcXaeMgmt Module folder (for testing purposes with the TcXaeMgmt Powershell Cmdlets).

- Using customized protocols for the Session object
- Browsing datatypes and Symbols
- Reading and Writing Values by Symbols and/or by IndexGroup/IndexOffset
- MEF Integration into the Beckhoff.TwinCAT.Ads package (actually TwinCAT Scope Browser, TcXaeMgmt Powershell module)

## Testing the Provider with the [TcXaeMgmt](https://www.powershellgallery.com/packages/TcXaeMgmt) Module
### Creating a Session in a CustomSessionProvider
```powershell
PS> $session = New-TcSession -Provider Custom -address Anything
PS> $session

ID Address  IsConnected ConnectionState Cycles Errors LastError EstablishedAt       LastSucceeded
-- -------  ----------- --------------- ------ ------ --------- -------------       -------------
11 Anything True        Connected                               07.12.2021 10:35:13
```
### Browsing DataTypes
```powershell
PS> ($session | get-tcdatatype).FullName
BuildInBool
BuildInInt
BuildInString
BuildInStruct
BuildInArray
```
### Browsing Symbols
```powershell
PS> ($session | get-tcSymbol -recurse).InstancePath
MAIN
MAIN.sym1
MAIN.sym2
MAIN.sym2.a
MAIN.sym2.b
MAIN.sym2.c
MAIN.sym2.d
MAIN.sym2.e
MAIN.sym3
Yoda
Yoda.Quota1
Yoda.Quota2
Yoda.Quota3
```
### Reading/Writing Symbol Values
``` powershell
PS> $session | Read-TcValue -Path Main.sym1
true
```

``` powershell
PS> $session | Write-TcValue -Path Main.sym1 -value $false
true
```