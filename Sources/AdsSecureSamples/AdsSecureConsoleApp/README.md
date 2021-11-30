## Description
Simple AdsSecure Console Application that demonstrates how to establish an ADSSecure channel to a remote TwinCAT System to read the ADSState.
The Authentication is done via a Self-Signed-Certificate.
## Requirements
- *.NET 5.0* or *.NETCORE3.1*
- *TwinCAT 3.1.4024* on remote system.
- No other System allocating the same port (e.g. a regular TwinCAT installation) on the local system

*CAUTION:* The **TlsConnect**, **X509CertificateHelper**, **AdsReadStateResponseHeader** objects are preliminary and are actually not documented.
They could be changed in future!

## Installation
It is necessary that the local (Sample-running-system) supports at least a TLS Ciphersuite that is used by TwinCAT.
### Actual TwinCAT supported Ciphersuites
```
TLS_DHE_RSA_WITH_AES_256_CBC_SHA256 (0x006b) (Not existing on Windows10?)
TLS_DHE_RSA_WITH_AES_256_CBC_SHA (0x0039) (Seems to be available)
TLS_DHE_RSA_WITH_AES_128_CBC_SHA256 (0x0067)
TLS_DHE_RSA_WITH_AES_128_CBC_SHA (0x0033)
```
### Windows Ciphersuites enabled by default
The List of actually enabled (and supported) ciphersuites can be determined by Powershell:
```Powershell
 PS> (get-TlsCiphersuite).Name

TLS_AES_256_GCM_SHA384
TLS_AES_128_GCM_SHA256
TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384
TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256
TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384
TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256
TLS_DHE_RSA_WITH_AES_256_GCM_SHA384
TLS_DHE_RSA_WITH_AES_128_GCM_SHA256
TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA384
TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256
TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384
TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256
TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA
TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA
TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA
TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA
TLS_RSA_WITH_AES_256_GCM_SHA384
TLS_RSA_WITH_AES_128_GCM_SHA256
TLS_RSA_WITH_AES_256_CBC_SHA256
TLS_RSA_WITH_AES_128_CBC_SHA256
TLS_RSA_WITH_AES_256_CBC_SHA
TLS_RSA_WITH_AES_128_CBC_SHA
TLS_RSA_WITH_3DES_EDE_CBC_SHA
TLS_RSA_WITH_NULL_SHA256
TLS_RSA_WITH_NULL_SHA
TLS_PSK_WITH_AES_256_GCM_SHA384
TLS_PSK_WITH_AES_128_GCM_SHA256
TLS_PSK_WITH_AES_256_CBC_SHA384
TLS_PSK_WITH_AES_128_CBC_SHA256
TLS_PSK_WITH_NULL_SHA384
TLS_PSK_WITH_NULL_SHA256
```

Here, on a Windows10 System there is no match by default. Trying to use the TlsConnect.Connect method will not open a secure SSL channel.

### Enabling TwinCAT appropriate CipherSuite
On Windows10 at least the *TLS_DHE_RSA_WITH_AES_256_CBC_SHA* variant is installed, but not enabled by default.
The CipherSuite can be enabled by the following command:

```Powershell
Ps> Enable-TlsCipherSuite -Name TLS_DHE_RSA_WITH_AES_256_CBC_SHA

PS> Get-TlsCipherSuite -Name TLS_DHE_RSA_WITH_AES_256_CBC_SHA

KeyType               : 0
Certificate           : RSA
MaximumExchangeLength : 1024
MinimumExchangeLength : 1024
Exchange              : DH
HashLength            : 160
Hash                  : SHA1
CipherBlockLength     : 16
CipherLength          : 256
BaseCipherSuite       : 57
CipherSuite           : 57
Cipher                : AES
Name                  : TLS_DHE_RSA_WITH_AES_256_CBC_SHA
Protocols             : {769, 770, 771, 65279…}
```

### Creating a Self-Signed Certificate
```powershell
PS> $cert = New-SelfSignedCertificate -DnsName TwinCATTestCertificate -CertStoreLocation cert:\LocalMachine\My
PS> $cert
 
   PSParentPath: Microsoft.PowerShell.Security\Certificate::LocalMachine\My

Thumbprint                                Subject              EnhancedKeyUsageList
----------                                -------              --------------------
9814BEADD027C50B5905DBD769D848D7EE777B78  CN=TwinCATTestCerti… {Client Authentication, Server Authentication}
```
### Browsing for Certs
```powershell
PS> $cert = Get-ChildItem -Path cert:\LocalMachine\My\ | where Subject -like *TwinCATTestCertificate
```
### Test Certificate
```powershell
PS> $cert | Test-Certificate -Policy SSL -AllowUntrustedRoot
```
### Deleting Certificates
```powershell
PS> $cert = Get-ChildItem -Path cert:\LocalMachine\My\ | where Subject -like *TwinCATTestCertificate

PS> $cert | Remove-Item
```
### Add TC3 certificate to sample code
The certificate of the TC3 remote target is stored in TwinCAT\3.1\Target\TcSelfSigned.xml. Add certificate and private key to static string variables s_certificate and s_rsaPrivateKey in Program.cs.
### Run the sample
Add remote netId (e.g. about dialog of TwinCAT systray icon), port (e.g. SystemService = 10000, PLC1 = 851), IP,  username/password and compare the fingerprint (e.g. about dialog of TwinCAT systray icon).

### Create certificates for CA AdsSecure
```powershell
# Create the ROOT CA Certificate
mkdir c:\certs
openssl genrsa -out C:\certs\RootCA.key 2048
openssl req -x509 -new -nodes -key C:\certs\RootCA.key -sha256 -subj "/C=DE/ST=NRW/L=Verl/O=Bk/OU=TCPM/CN=RootCA" -days 3600 -out C:\certs\RootCA.pem

# Create the Certificate for a IPC
openssl genrsa -out C:\certs\TargetIPC.key 2048
openssl req -out C:\certs\TargetIPC.csr -key C:\certs\TargetIPC.key -subj "/C=DE/ST=NRW/L=Verl/O=Bk/OU=TCPM/CN=TargetIPC" –new
openssl x509 -req -in C:\certs\TargetIPC.csr -CA C:\certs\RootCA.pem -CAkey C:\certs\RootCA.key -CAcreateserial -out C:\certs\TargetIPC.crt -days 360 -sha256
```

## Using the sample with Certification Authority (CA) certificates
Using a CA Certificate enforces that the RootCA certificate is known on both sides, the SourceIPC and the TargetIPC.
The SourceIPC side is hardcoded already in this sample (s_certificate, Program.s_ca, and Program.s_rsaPrivateKey variables).
The TargetIPC side (if a running TwinCAT system) must have the target-side certificates derived from the RootCA certificate.

Copy the files
.\Certs\RootCA.pem
.\Certs\TargetIPC.crt
.\Certs\TargetIPC.key

to the target system (e.g to the c:\twincat\3.1\target\CACerts\ folder) and
configure the c:\twincat\3.1\target\StaticRoutes.xml file as follows:

```xml
<?xml version="1.0"?>
<TcConfig xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
	<RemoteConnections>
		<Server>
			<tls>
				<ca>c:\twincat\3.1\target\CACerts\RootCA.pem</ca>
				<cert>c:\twincat\3.1\target\CACerts\TargetIPC.crt</cert>
				<key>c:\twincat\3.1\target\CACerts\TargetIPC.key</key>
			</tls>
		</Server>
	</RemoteConnections>
</TcConfig>
```

Restart or Reconfig the TwinCAT System Service to read the StaticRoutes.xml configuration.

Deactivate the SelfSigned mode in source code:
```csharp
static async Task Main(string[] args)
{
    // Change this flag to change between SelfSigned and CA certificates!
    bool selfSigned = false;
	...
}
```

Run this sample application and enter the Address the of the IPC target system (AmsNetId and Port 10000).
Alternatively, the sample application can be started with arguments:

```
PS> AdsSecureConsoleApp [NETID] [PORT] [IPORHOSTNAME]
```

## Using the sample with SelfSigned AdsSecure connection
For SelfSigning, the access to the target system is authenticated by the Source system certificate plus the Target System credentials.
Therefore the already included Certificate in the source sample application can be used and no changes are necessary at the target system AdsSecure configuration (StaticRoutes.xml).


On the target IPC side, an empty StaticRoutes.xml should be enough:

```xml
<?xml version="1.0"?>
<TcConfig xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
	<RemoteConnections>
	</RemoteConnections>
</TcConfig>
```

Activate the SelfSigned mode in source code:
```csharp
static async Task Main(string[] args)
{
    // Change this flag to change between SelfSigned and CA certificates!
    bool selfSigned = true;
	...
}
```
Run the application.



