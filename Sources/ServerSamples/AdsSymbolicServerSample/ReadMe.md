# Using of the AdsSymbolServerSample

Run the AdsSymbolicServerSample application
The AmsPort is 25000.

An easy way to test the server is using the **TcXaeMgmt** Powershell module.

## Installing the **TcXaeMgmt** Powershell Module

If the **TcXaeMgmt** Module is not already existing on the system, it 
can be installed from the (Powershell Gallery)[https://www.powershellgallery.com]:

To test the availability:

```powershell
get-module TcXaeMgmt -listAvailable
```

The module is available in different flavours to support

- Windows Powershell >= 4.0
- Powershell (Core) >= 6.0

and different TwinCAT (Tc2 and Tc3) versions.

(TcXaeMgmt Package)[https://www.powershellgallery.com/packages/TcXaeMgmt]
(Beckhoff Documentation)[https://infosys.beckhoff.com/content/1033/tc3_ads_ps_tcxaemgmt/3972231819.html?id=8731138690123386389]

Please check the installation hints and install the module via PowershellGet Package Manager:

```powershell
PS> install-module TcXaeMgmt
PS> get-module TcXaeMgmt -listAvailable
```

## Testing the custom Ads Server **AdsSymbolServer**

### Getting AdsState

```powershell
PS> $session = new-tcsession -NetId Local -port 25000
PS> $session | Get-AdsState

Name    State OK   Time (ms) Address
----    ----- --   --------- -------
CX_1234 Run   True 20        172.17.60.167.1.1:25000
```

### Reading/Writing Values

Read a primitive value:

```powershell
PS> $session = new-tcsession -NetId Local -port 25000
PS> $session | Read-TcValue -path Main.string1
Hello world!
```

Writing a primitive value:

```powershell
PS> $session = new-tcsession -NetId Local -port 25000
PS> $session | Write-TcValue -path Main.string1 -Value 'New written Value' -force
```

```powershell
PS> $session | Read-TcValue -path Main.string1
New written Value
```

Read a struct value:

```powershell
PS> $session | read-tcValue -path 'Main.RpcInvoke1'

name    : Main.rpcInvoke1
a       : False
b       : 555
c       : 666
PSValue : ...
```

### Notifications
In this sample all Symbols are registered for AdsNotifactions by default. Therefore, every write should create a notification output in the console.

```powershell
$session | write-tcValue -path 'Main.string1' -Value 'New Value!' -force
```

Output in console window:

```cmd
WhenNotification Symbol 'Main.string1' changed to value 'New Value!'
```

### Browsing DataTypes and Symbols/Instances

```powershell
PS> test-adsroute -port 25000

Name                 Address           Port    Latency Result
                                               (ms)
----                 -------           -----   ------- ------
CX_1234              172.17.60.167.1.1 25000   36      Ok
```

```powershell
PS> $session = new-tcsession -NetId Local -port 25000
PS> $session | get-tcDataType

Name                      Size     Category   BaseType
----                      ----     --------   --------
BOOL                      1        Primitive
INT                       2        Primitive
DINT                      4        Primitive
WSTRING(80)               162      String
MYSTRUCT                  169      Struct
ARRAY [0..3][0..1] OF INT 16       Array      INT
MYENUM                    4        Enum       DINT
MYALIAS                   4        Alias      MYENUM
POINTER TO INT            8        Pointer    INT
REFERENCE TO INT          8        Reference  INT
MYRPCSTRUCT               169      Struct
BYTE                      1        Primitive
PCCH                      8        Pointer    BYTE
```

```powershell
PS> $session | Get-TcSymbol -recurse

InstancePath            Category  DataType                  Size Static Persistant IG   IO
------------            --------  --------                  ---- ------ ---------- --   --
Globals                 Struct                              0    False  False      0    0
Globals.bool1           Primitive BOOL                      1    False  False      2    1000
Globals.int1            Primitive INT                       2    False  False      2    1001
Globals.dint1           Primitive DINT                      4    False  False      2    1003
Globals.string1         String    WSTRING(80)               162  False  False      2    1007
Globals.myStruct1       Struct    MYSTRUCT                  169  False  False      2    10A9
Globals.myStruct1.name  String    WSTRING(80)               162  False  False      2    10A9
Globals.myStruct1.a     Primitive BOOL                      1    False  False      2    114B
Globals.myStruct1.b     Primitive INT                       2    False  False      2    114C
Globals.myStruct1.c     Primitive DINT                      4    False  False      2    114E
Globals.myArray1        Array     ARRAY [0..3][0..1] OF INT 16   False  False      2    1152
Globals.myEnum1         Enum      MYENUM                    4    False  False      2    1162
Globals.myAlias1        Alias     MYALIAS                   4    False  False      2    1166
Globals.pointer1        Pointer   POINTER TO INT            8    False  False      2    116A
Globals.pointer1^       Primitive INT                       2    False  False      F014 0
Globals.reference1      Reference REFERENCE TO INT          8    False  False      2    1172
Globals.rpcInvoke1      Struct    MYRPCSTRUCT               169  False  False      2    117A
Globals.rpcInvoke1.name String    WSTRING(80)               162  False  False      2    117A
Globals.rpcInvoke1.a    Primitive BOOL                      1    False  False      2    121C
Globals.rpcInvoke1.b    Primitive INT                       2    False  False      2    121D
Globals.rpcInvoke1.c    Primitive DINT                      4    False  False      2    121F
Main                    Struct                              0    False  False      0    0
Main.bool1              Primitive BOOL                      1    False  False      1    1000
Main.int1               Primitive INT                       2    False  False      1    1001
Main.dint1              Primitive DINT                      4    False  False      1    1003
Main.string1            String    WSTRING(80)               162  False  False      1    1007
Main.myStruct1          Struct    MYSTRUCT                  169  False  False      1    10A9
Main.myStruct1.name     String    WSTRING(80)               162  False  False      1    10A9
Main.myStruct1.a        Primitive BOOL                      1    False  False      1    114B
Main.myStruct1.b        Primitive INT                       2    False  False      1    114C
Main.myStruct1.c        Primitive DINT                      4    False  False      1    114E
Main.myArray1           Array     ARRAY [0..3][0..1] OF INT 16   False  False      1    1152
Main.myEnum1            Enum      MYENUM                    4    False  False      1    1162
Main.myAlias1           Alias     MYALIAS                   4    False  False      1    1166
Main.pointer1           Pointer   POINTER TO INT            8    False  False      1    116A
Main.pointer1^          Primitive INT                       2    False  False      F014 0
Main.reference1         Reference REFERENCE TO INT          8    False  False      1    1172
Main.rpcInvoke1         Struct    MYRPCSTRUCT               169  False  False      1    117A
Main.rpcInvoke1.name    String    WSTRING(80)               162  False  False      1    117A
Main.rpcInvoke1.a       Primitive BOOL                      1    False  False      1    121C
Main.rpcInvoke1.b       Primitive INT                       2    False  False      1    121D
Main.rpcInvoke1.c       Primitive DINT                      4    False  False      1    121F
```

### Invoking Rpc methods

Get the symbol with RpcMethods (RpcStruct):

```powershell
 PS> $rpcSymbol = $s | get-tcSymbol -path 'Main.RpcInvoke1'
 PS> $rpcSymbol

InstancePath    Category DataType    Size Static Persistant IG IO
------------    -------- --------    ---- ------ ---------- -- --
Main.rpcInvoke1 Struct   MYRPCSTRUCT 169  False  False      1  117A

```

Get the (dynamic) fields and methods of the symbol:

```powershell
PS> > $rpcSymbol | get-member -MemberType dynamic

   TypeName: TwinCAT.TypeSystem.DynamicRpcStructInstance

Name                  MemberType Definition
----                  ---------- ----------
a                     Dynamic    dynamic a
b                     Dynamic    dynamic b
c                     Dynamic    dynamic c
Method1               Dynamic    dynamic Method1
Method1Async          Dynamic    dynamic Method1Async
Method2               Dynamic    dynamic Method2
Method2Async          Dynamic    dynamic Method2Async
Method3               Dynamic    dynamic Method3
Method3Async          Dynamic    dynamic Method3Async
Method4               Dynamic    dynamic Method4
Method4Async          Dynamic    dynamic Method4Async
Method5               Dynamic    dynamic Method5
Method5Async          Dynamic    dynamic Method5Async
name                  Dynamic    dynamic name
```

Call the dynamic method:

```powershell
PS> 0> $rpcSymbol.Method1(4,5)
9
```
