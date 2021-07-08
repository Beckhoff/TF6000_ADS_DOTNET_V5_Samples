# Using of the AdsSymbolServerSample

Run the AdsSymbolicServerSample application
The AmsPort is 6000. An easy way to test the server is using the **TcXaeMgmt** Powershell module.

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
PS> $session = new-tcsession -NetId Local -port 6000
PS> $session | Get-AdsState

Name    State OK   Time (ms) Address
----    ----- --   --------- -------
CX_1234 Run   True 20        172.17.60.167.1.1:6000
```
### Reading/Writing Values

```powershell
PS> $session = new-tcsession -NetId Local -port 6000
PS> $session | Read-TcValue -path Main.string1
Hello world!
```

```powershell
PS> $session = new-tcsession -NetId Local -port 6000
PS> $session | Write-TcValue -path Main.string1 -Value 'New written Value' -force
```

```powershell
PS> $session | Read-TcValue -path Main.string1
New written Value
```

### Browsing DataTypes and Symbols/Instances
```powershell
PS> test-adsroute -port 6000

Name                 Address           Port   Latency Result
                                               (ms)
----                 -------           ----   ------- ------
CX_1234              172.17.60.167.1.1 6000   36      Ok
```
```powershell
PS> $session = new-tcsession -NetId Local -port 6000
PS> $session | get-tcDataType

Name                      Size     Category   BaseType
----                      ----     --------   --------
BOOL                      1        Primitive
INT                       2        Primitive
DINT                      4        Primitive
WSTRING(80)               162      String
MYSTRUCT                  7        Struct
ARRAY [0..3][0..1] OF INT 16       Array      INT
MYENUM                    4        Enum       DINT
MYALIAS                   4        Alias      MYENUM
POINTER TO INT            8        Pointer    INT
REFERENCE TO INT          8        Reference  INT
```
```powershell
PS> $session | Get-TcSymbol -recurse

InstancePath        Category  DataType                  Size Static Persistant IG   IO
------------        --------  --------                  ---- ------ ---------- --   --
Globals             Struct                              0    False  False      0    0
Globals.bool1       Primitive BOOL                      1    False  False      2    1000
Globals.int1        Primitive INT                       2    False  False      2    1001
Globals.dint1       Primitive DINT                      4    False  False      2    1003
Globals.string1     String    WSTRING(80)               162  False  False      2    1007
Globals.myStruct1   Struct    MYSTRUCT                  7    False  False      2    10A9
Globals.myStruct1.a Primitive BOOL                      1    False  False      2    10A9
Globals.myStruct1.b Primitive INT                       2    False  False      2    10AA
Globals.myStruct1.c Primitive DINT                      4    False  False      2    10AC
Globals.myArray1    Array     ARRAY [0..3][0..1] OF INT 16   False  False      2    10B0
Globals.myEnum1     Enum      MYENUM                    4    False  False      2    10C0
Globals.myAlias1    Alias     MYALIAS                   4    False  False      2    10C4
Globals.pointer1    Pointer   POINTER TO INT            8    False  False      2    10C8
Globals.pointer1^   Primitive INT                       2    False  False      F014 0
Globals.reference1  Reference REFERENCE TO INT          8    False  False      2    10D0
Main                Struct                              0    False  False      0    0
Main.bool1          Primitive BOOL                      1    False  False      1    1000
Main.int1           Primitive INT                       2    False  False      1    1001
Main.dint1          Primitive DINT                      4    False  False      1    1003
Main.string1        String    WSTRING(80)               162  False  False      1    1007
Main.myStruct1      Struct    MYSTRUCT                  7    False  False      1    10A9
Main.myStruct1.a    Primitive BOOL                      1    False  False      1    10A9
Main.myStruct1.b    Primitive INT                       2    False  False      1    10AA
Main.myStruct1.c    Primitive DINT                      4    False  False      1    10AC
Main.myArray1       Array     ARRAY [0..3][0..1] OF INT 16   False  False      1    10B0
Main.myEnum1        Enum      MYENUM                    4    False  False      1    10C0
Main.myAlias1       Alias     MYALIAS                   4    False  False      1    10C4
Main.pointer1       Pointer   POINTER TO INT            8    False  False      1    10C8
Main.pointer1^      Primitive INT                       2    False  False      F014 0
Main.reference1     Reference REFERENCE TO INT          8    False  False      1    10D0
```
