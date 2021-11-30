using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Security;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads;
using TwinCAT.Ams;
using TwinCAT.Tls;


namespace AdsSecureConsoleApp
{
    //public enum TlsType
    //{
    //    SelfCert,
    //    CA
    //}

    class Program
    {
        /// <summary>
        /// The local AmsNetId (of the SourceIPC/ClientIPC)
        /// </summary>
        static AmsNetId s_localNetId = new AmsNetId("1.1.1.1.1.1");


        /// <summary>
        /// SourceIPC Certificate as string (PEM-Format, copied from .\Certs\SourceIPC.crt)
        /// </summary>
        static string s_certificate = @"-----BEGIN CERTIFICATE-----
MIIDODCCAiACFDFkqPhIY5TR1kSDldjD6YgIpBcuMA0GCSqGSIb3DQEBCwUAMFcx
CzAJBgNVBAYTAkRFMQwwCgYDVQQIDANOUlcxDTALBgNVBAcMBFZlcmwxCzAJBgNV
BAoMAkJrMQ0wCwYDVQQLDARUQ1BNMQ8wDQYDVQQDDAZSb290Q0EwHhcNMjExMTI2
MTYzODAwWhcNMjIxMTIxMTYzODAwWjBaMQswCQYDVQQGEwJERTEMMAoGA1UECAwD
TlJXMQ0wCwYDVQQHDARWZXJsMQswCQYDVQQKDAJCazENMAsGA1UECwwEVENQTTES
MBAGA1UEAwwJU291cmNlSVBDMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKC
AQEA5QRND9VwXGuSM9AW7SsWPpLbCToi8MB1qOS/MZ25wJQUjblH+oeF4qHep5oe
vSYE+dDkiDHQ2wqgnSL1sT0c0hd80GrUFDVwO65O/T2YNX2fw/dGCD8kfvdpIECE
lQ+n8OFu55mhEauHHMEUV0tWlXS8Li5zVws+bkdGlJu4d3rnSDUmVSVtiNI801K5
IBtBwHA6Mhs/0k2DCMaOk949NpzVUUhNLo83r6Zj3vC1gjXZetoMplly2gY4NRos
Yxorr95WKcTPHBzsJcUvMqGKEHTkLnbG7DfTw4fPPack7uewFb2sbKVnJdkiGOAC
oYZl9XQoPQWUIhMOylYU/Q2eTQIDAQABMA0GCSqGSIb3DQEBCwUAA4IBAQBWxQ9A
vtlKEpL5N/+h/4xahywayygRNMN6GAoZM2NE1OAm/nBNFbk6AS1hhczYppFG4f7U
VqELjK0oUB9xUMCEidxDFOm3PDjtTu2h2Nx0C4LlT/YvEsX9WR+3Du7nZ+Z0+jd9
v0jwbRn/t9xPOOZswo1I/p/OtQXH4tdbcWozG0pKeTpubkmMaBgFXUDv5ePPxrlH
Gr+E6MYtlrZtDxEywt2JC2KK7Snr/528FBvArTdaS0B75nE28EuBiRDxs88iThql
TKKtRdrkm/zqNYOYnTIQl5iXfyZrNFTPhrF+GpCGwyWqOLihQLoRNYb5B0q8d4vJ
IcPKu8e1NRUosGUE
-----END CERTIFICATE-----";

       /// <summary>
        /// RSA Private key as string (PEM format, copied from .\certs\SourceIPC.key)
        /// </summary>
        static string s_rsaPrivateKey = @"-----BEGIN RSA PRIVATE KEY-----
MIIEowIBAAKCAQEA5QRND9VwXGuSM9AW7SsWPpLbCToi8MB1qOS/MZ25wJQUjblH
+oeF4qHep5oevSYE+dDkiDHQ2wqgnSL1sT0c0hd80GrUFDVwO65O/T2YNX2fw/dG
CD8kfvdpIECElQ+n8OFu55mhEauHHMEUV0tWlXS8Li5zVws+bkdGlJu4d3rnSDUm
VSVtiNI801K5IBtBwHA6Mhs/0k2DCMaOk949NpzVUUhNLo83r6Zj3vC1gjXZetoM
plly2gY4NRosYxorr95WKcTPHBzsJcUvMqGKEHTkLnbG7DfTw4fPPack7uewFb2s
bKVnJdkiGOACoYZl9XQoPQWUIhMOylYU/Q2eTQIDAQABAoIBAHihA1ZLvptvrdrM
yMaz40uiXu1FShI1zcPgeTTRN35QgWMFLPyxVbxCNt1hOL+4vvY3KajzVGeL+X3L
ZE6vOfcPmBbPrlXWr/8/mSuavgmu2fCG1sSRPdAC0cTDNxKs5HDkzV4Ade6wwsJK
kURJ0pl2m4hXvzkiGwhLUsoEG+4SN4J57Oikuf0J0ZoVs3+TmzTclpbZ0JfSn2+0
RHfad+nNpxrzwgD+Avybkvt7pnmVJeHuh09ICN3yV49Cu1zABf1BesCiFhEsFEuU
RFwsMqhJ18bv1znGFQd2XqSu6AQfbfOkwRvxKy+Z4pEK/HYjLyH6GeD8VUEd9Utm
fOmDfAECgYEA+RHkK+pefIsdmRS5vU8ruEb1+pnqA2dDUit7bsHJZLXT4unl1XWB
lqkMQSAdKIlrwzTbzWGgtfEulcMPqLl8yH/40ZEdF7WA+SYATsLLqQt1KeNMR4aE
NgoTCaVW6nnQneJ1i8acI445gmJPCwG4puH1FQ+Zm0lXYG3elYzKLO0CgYEA62OS
qKToBt61TWS4kGS6avn7vFXrh6JAEO3CV61OVhGQKc/H0oTJBDPhRd/h1YznhILz
FZPAVK9yy6qfd59x0ggmmcK9+VlsJlH+Tnylaq+2Geyo0gdzE4IyVrA9n7UrG8nl
MVCHGuM6obVl2rkjrGiuoJehy6hbZ1XppHWKauECgYBIzSX4gCTmGnOoTxqLbxzE
XFmByoNQQ1q2JeeKVDJdsZghd2SqpBIgy4C9eHmNY72P7V9iBOtIwxpuw/lLxAvp
Px6ngtcSGwd7y9PDMcT9wE+a0sl1DqiOcxtlcmKZXsnPnGXnWUJCUkwVBE8+VF54
yQsuAMVRUnqrwPGSnPhrcQKBgQDG6WTkmD3umEJTPVrtwgD6J3dABsc63bQP2isR
VkVNXBgcDRaJ4mXP5FtoZbF8eU6nXtU2FZ5AseZrDyskthtD5llgM/2/eX53v3AM
OS67wfI7ZA6hNWRcRvhs4w+gJ0NffzPrgWY6JWzFe/mvZCYuKmPvF1PFOubKowIG
VMF8YQKBgE85Ht4dyEoNrVH+WlrCl9xB6HWdkXMl2ZFh+YFJlKwEUuheto4nTdWn
p8rc2I9y9PX7u8kNYAKb9ppsX/Cq+0j89ahJl0QJkZZZYCIK1dHtpHUxNLgoWKyR
2iP8DpIry0uuIoi/VkoG/M1c9+w4Ojn2x9E3a7PIBK7M4H1F6w1e
-----END RSA PRIVATE KEY-----";

        /// <summary>
        /// RootCA certificate (PEM-Format, copied from .\certs\RootCA.pem)
        /// </summary>
        static string s_ca = @"-----BEGIN CERTIFICATE-----
MIIDjzCCAnegAwIBAgIULoxUT8ves7P+ftlFpdVrG7yOUTMwDQYJKoZIhvcNAQEL
BQAwVzELMAkGA1UEBhMCREUxDDAKBgNVBAgMA05SVzENMAsGA1UEBwwEVmVybDEL
MAkGA1UECgwCQmsxDTALBgNVBAsMBFRDUE0xDzANBgNVBAMMBlJvb3RDQTAeFw0y
MTExMjYxNjM3NDRaFw0zMTEwMDUxNjM3NDRaMFcxCzAJBgNVBAYTAkRFMQwwCgYD
VQQIDANOUlcxDTALBgNVBAcMBFZlcmwxCzAJBgNVBAoMAkJrMQ0wCwYDVQQLDARU
Q1BNMQ8wDQYDVQQDDAZSb290Q0EwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEK
AoIBAQDufF+lO4Rv++ycuvMX7tiLoM3woWJmOxurzwr8YgTs89Fu129VC8jpRQhW
FROLjRC7mtwM/EBjXHlPGrXKqL6jqBorAiA+j/pqfgBU5ciUm8FOAih2gpjDbj1A
0jQ4RhPtXYVAB8zNVpMvNULZq4go9z1/xnGBzuCYTC92faP2PeOzESHmYG5/5+Hj
CDu8xiFLtDZf/cPpyBd5YF3XYq6JZuvHRkYd4UQUTWgZ+hU5C5XapPfbGa+NGyOv
OffjCL+gN6wve0el4G/3Le8jt6uvC3qZ+wgF5gUypFp0frwpWqfj9HDXeU9x4xEU
XwLZDZEEAqmz9AoAnvWINCKrMOtPAgMBAAGjUzBRMB0GA1UdDgQWBBSVMYnuU5Z0
+hyFBvsLmH0mEdR/LTAfBgNVHSMEGDAWgBSVMYnuU5Z0+hyFBvsLmH0mEdR/LTAP
BgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQA+MlgRpl49RmtDZyYW
GayI3kstGq/YUYEI+5lkNXuJ8mWd36WzGBTDaQHALsxkXpkwv0V8ed7bGyJT4iOh
wIdMaOU49e5uY1M7pjB3O/mI+Q2LJD6D8z1ENk4jK0H4EyYUsyNn5Tc5Uc8YCMxb
P8bevlpCjDTYmeVcBREWc7M2P6nIW7IDjx9RaJO7nHgQFHtRtG+lpJwC2NcQhoGm
3joECM5rQZt7jPY8SR9loo9iqR824zRAC2E+glDrd9cSlt8MV825wHed6W/6Cfsm
+X21yG7Z0ghzyba3WdKh9ByeIxFM6CYgTSfpn3ySowX/5vj4UUodMqbrWKjU8nn5
ASqJ
-----END CERTIFICATE-----";


        /// <summary>
        /// Wrong RootCA certificate (PEM-Format, copied from .\certs\WrongRootCA.pem)
        /// Use this, to do a negative test (different Root CAs on Source and target)
        /// </summary>
        static string s_wrongCa = @"-----BEGIN CERTIFICATE-----
MIIDmTCCAoGgAwIBAgIUBUFgxVsj94RMfG9VRdEnMbnRy3gwDQYJKoZIhvcNAQEL
BQAwXDELMAkGA1UEBhMCREUxDDAKBgNVBAgMA05SVzENMAsGA1UEBwwEVmVybDEL
MAkGA1UECgwCQmsxDTALBgNVBAsMBFRDUE0xFDASBgNVBAMMC1dyb25nUm9vdENB
MB4XDTIxMTEyOTEyMjYwMVoXDTMxMTAwODEyMjYwMVowXDELMAkGA1UEBhMCREUx
DDAKBgNVBAgMA05SVzENMAsGA1UEBwwEVmVybDELMAkGA1UECgwCQmsxDTALBgNV
BAsMBFRDUE0xFDASBgNVBAMMC1dyb25nUm9vdENBMIIBIjANBgkqhkiG9w0BAQEF
AAOCAQ8AMIIBCgKCAQEAx4vwp88gYxwN5jqUdxtobpJ56QVnwhbY7oeQd+XcPycd
UTPxolK/lMYkjqisv6OwWKTBJjLTrczGeg37DzYtoTa6Av7wIOdMT/odUCxJM0PW
wpUaEXQerCkPDsJL9AcMGMy88efPMurYhSv1dzJgDfzSiWoU+yI9EmntLk0tjLP6
P2Z2AhLt5gRXGiZnYpYAi7/C+LOz1v9luUBZ207D5RfnS24gvMPBHpxfXv6opjMu
mDlv/BBwD3mo0t+zYsHSzYuDwyrf7XWdZmgU6ObJk/oY73HVJDvq+uQ0XvQDK5oF
lW3+C/NEUH7mBrQRkMXfnqyN3gDeL4voJOkq5gNmGwIDAQABo1MwUTAdBgNVHQ4E
FgQUJYsmB+RTHNp3VHug+B6iodHI8UQwHwYDVR0jBBgwFoAUJYsmB+RTHNp3VHug
+B6iodHI8UQwDwYDVR0TAQH/BAUwAwEB/zANBgkqhkiG9w0BAQsFAAOCAQEARFrI
2UDoOi2/2WEL9CNJ4f+9q92xmi/lOmL+NBNRoDj7gYsIG/PzwX+idm1xanO7wGca
Pufq8aGTgK1f4eRNdx986apTFQ+dzSrOLoHJvc7e1EIdneWCVhEzVz+/0G5zikRD
agJ1ZjLj7N6WNdviYFbb8EvxY29yBH0kjrr8UCNOC0uwwhMz5deK3Cw5hWyDnWWz
p5MfVvouhzi2jHKIINhf5tWxhmB9OU+97H02Vr++dDiwJQiuGV41rnG12xkA9eUQ
ceTFLVIXa+e7oyZxMZV+WC+nG6FApVAV2Hn7nuSK2xnVe8FLw1OzV6iioZMFXqRI
Tb3cOEnlLuwCqOJb3g==
-----END CERTIFICATE-----";


        static async Task Main(string[] args)
        {
            // Change this flag to change between SelfSigned and CA certificates!
            bool selfSigned = false;

            AmsAddress targetAddress;
            string ipOrHostName;
            bool parsed = ArgParser.TryParse(args, out targetAddress, out ipOrHostName);

            CancellationToken cancel = CancellationToken.None;

            if (!parsed)
            {
                targetAddress = ReadAmsAddressFromConsole();
                ipOrHostName = ReadIPOrHostNameFromConsole();
            }

            // Create CA Certificate from PEM Formatted string
            X509Certificate2 caCert = X509CertificateHelper.CreateFromPem(s_ca);
            
            // Use an mismatching CARoot Certifcate for negative test.
            //X509Certificate2 caCert = X509CertificateHelper.CreateFromPem(s_wrongCa);

            // Loading Cert from PEM Formatted strings:
            X509Certificate2 certFromStrings = X509CertificateHelper.CreateFromPem(s_certificate, s_rsaPrivateKey);

#if FROM_STORE
            // Or alternatively (more convenient on Windows)
            // Get the Certificate (*.pfx) from Cert Store or from File:
            X509Certificate2 certFromStore;
            using (X509Store store = new X509Store(StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection cers = store.Certificates.Find(X509FindType.FindBySubjectName, "TwinCATTestCertificate", false);
                if (cers.Count > 0)
                {
                    certFromStore = cers[0];
                };
            }
#endif

            TlsConnect connect = null; // Tls Connection object.
                if (selfSigned)
                {
                    (string user, SecureString password) credentials = ReadCredentialsFromConsole();
                    connect = new TlsConnectSelfSigned(s_localNetId, ipOrHostName, certFromStrings, credentials.user, credentials.password);
                }
                else
                {
                    connect = new TlsConnectCA(s_localNetId, ipOrHostName, caCert, certFromStrings);
                }
            try
            {
                await connect.ConnectAsync(targetAddress, cancel); // Connect Asyncronously

                SslStream sslStream = connect.GetStream();

                string serverHostName = string.Empty;

#if NET5_0_OR_GREATER
                serverHostName = sslStream.TargetHostName;
#endif
                string serverSubject = sslStream.RemoteCertificate.Subject;
                string serverIssuer = sslStream.RemoteCertificate.Issuer;
                //string sha1Thumbprint = sslStream.RemoteCertificate.GetCertHashString();
                string sha256FingerPrint = sslStream.RemoteCertificate.GetSha256Thumbprint();

                Console.WriteLine($"Connected to RemoteServer '{serverHostName}' (Subject: {serverSubject}, Issuer: {serverIssuer}");
                Console.WriteLine($"RemoteServer Fingerprint: {sha256FingerPrint}");

                bool trustTarget = false;

                if (selfSigned)
                {
                    //Ensure that the Server/Target Fingerprint is correct.
                    //We have to compare the Fingerprint to trust the AdsServer/Target
                    trustTarget = ValidateFingerprint(sha256FingerPrint);
                }
                else
                {
                    // CA Certification is safe/trusted in itself (both communication ends are certified by the same RootCA)
                    trustTarget = true;
                }

                if (trustTarget)
                {
                    // Timeout after 5 Seconds
                    CancellationTokenSource timeoutCancelSource = new CancellationTokenSource(5000); 
                    Console.WriteLine("");

                    try
                    {
                        AdsState state = await ReadStateAsync(connect, timeoutCancelSource.Token);
                        Console.WriteLine($"SUCCESS: The Ads state of target {targetAddress} is '{state}'.");
                    }
                    catch (ApplicationException ex)
                    {
                        Console.WriteLine($"FAILED: Cannot read AdsState from target {targetAddress}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
            connect.Close();
        }

        /// <summary>
        /// Show and Validate the Fingerprint via Console.
        /// </summary>
        /// <param name="fingerprint">The fingerprint.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool ValidateFingerprint(string fingerprint)
        {
            bool trustTarget = false;
            Console.WriteLine($"Is the Server/Target Fingerprint '{fingerprint}' correct (y/n)?");
            ConsoleKeyInfo consoleKey = Console.ReadKey();

            if (consoleKey.KeyChar == 'y' || consoleKey.KeyChar == 'Y')
            {
                trustTarget = true;
            }
            return trustTarget;
        }

        /// <summary>
        /// Read the target system Credentials from the Console
        /// </summary>
        /// <returns>System.ValueTuple&lt;System.String, SecureString&gt;.</returns>
        private static (string userName, SecureString pwd) ReadCredentialsFromConsole()
        {
            string userName;
            SecureString password = new SecureString();

            Console.Write("UserName: ");

            do
            {
                userName = Console.ReadLine();
            } while (string.IsNullOrEmpty(userName));

            ConsoleKeyInfo key;

            Console.Write("Enter password: ");
            do
            {
                key = Console.ReadKey(true);

                // Ignore any key out of range.
                //if (((int)key.Key) >= 65 && ((int)key.Key <= 90))
                if (((int)key.Key) >= 0x20 && ((int)key.Key <= 0x7E)) // Printable characters
                {
                    // Append the character to the password.
                    password.AppendChar(key.KeyChar);
                    Console.Write("*");
                }
                // Exit if Enter key is pressed.
            } while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return (userName, password);
        }

        /// <summary>
        /// Read the target IPAddress/HostName from Console Input
        /// </summary>
        /// <returns>System.String.</returns>
        private static string ReadIPOrHostNameFromConsole()
        {
            string hostOrIp;

            do
            {
                Console.Write("Enter Target HostName/IPAddress: ");
                hostOrIp = Console.ReadLine();
            } while (string.IsNullOrEmpty(hostOrIp));
            return hostOrIp;
        }

        /// <summary>
        /// Reads target AmsAddress from Console Input
        /// </summary>
        /// <returns>AmsAddress.</returns>
        private static AmsAddress ReadAmsAddressFromConsole()
        {
            AmsNetId targetNetId = null;
            int targetPort = 0;

            bool ok = false;

            do
            {
                Console.Write("Enter Target NetId: ");
                string targetNetIdStr = Console.ReadLine();

                ok = AmsNetId.TryParse(targetNetIdStr, out targetNetId);
            } while (!ok);

            targetPort = 0;
            do
            {
                Console.Write("Enter Target AdsPort: ");
                string targetPortStr = Console.ReadLine();
                ok = int.TryParse(targetPortStr, out targetPort);
            } while (!ok);

            return new AmsAddress(targetNetId, targetPort);
        }

        /// <summary>
        /// The ReadState Operation (asynchronous)
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="cancel">The cancel.</param>
        /// <returns>AdsState.</returns>
        /// <exception cref="ApplicationException">ReadStateAsync failed with '{adsError}'!</exception>
        static async Task<AdsState> ReadStateAsync(TlsConnect connection, CancellationToken cancel)
        {
            AdsState state = AdsState.Invalid;
            
            // Create AmsHeader
            AmsHeader header = new AmsHeader(connection.TargetAddress, connection.LocalAddress, AdsCommandId.ReadState, AmsStateFlags.AdsCommand, 0, 0, 1);

            // Create the AmsCommand for ReadState (ReadState dosn't have data content)
            AmsCommand command = new AmsCommand(header, Array.Empty<byte>());
            int size = AmsCommandFrameMarshaller.MarshalSize(command);

            byte[] requestBuffer = new byte[size];
            // Marshal Command Data to buffer
            int marshalledBytes = AmsCommandFrameMarshaller.Marshal(command, requestBuffer.AsSpan());

            SslStream securedStream = connection.GetStream();
            await securedStream.WriteAsync(requestBuffer, cancel).ConfigureAwait(false);

            // Create Response Buffer of adequate size
            int length = AmsHeader.MarshalSize + AdsReadStateResponseHeader.StaticMarshalSize;
            byte[] responseBuffer = new byte[length];
            
            // Read the Response asynchrously
            int returnedBytes = await securedStream.ReadAsync(responseBuffer, cancel).ConfigureAwait(false);

            AmsHeader responseHeader;
            
            // Unmarshal data to AmsHeader
            int offset = AmsHeaderMarshaller.Unmarshal(responseBuffer.AsSpan(0), out responseHeader);

            AdsErrorCode adsError = (AdsErrorCode)BinaryPrimitives.ReadUInt32LittleEndian(responseBuffer.AsSpan(offset, 4));
            offset += 4;

            // Unmarshal the DataContent (here 4 Bytes)
            if (adsError == AdsErrorCode.NoError)
            {
                state = (AdsState)BinaryPrimitives.ReadUInt16LittleEndian(responseBuffer.AsSpan(offset, 2));
                offset += 2;
                ushort deviceState = BinaryPrimitives.ReadUInt16LittleEndian(responseBuffer.AsSpan(offset, 2));
                offset += 2;
                return state;
            }
            else
            {
                throw new ApplicationException($"ReadStateAsync failed with '{adsError}'!");
            }
        }
    }

    /// <summary>
    /// Extend the X509Certificate class
    /// </summary>
    public static class X509CertificateExtension
    {
        /// <summary>
        /// Gets the sha256 thumbprint (fingerprint from the certificate)
        /// </summary>
        /// <param name="cert">The cert.</param>
        /// <returns>System.String.</returns>
        public static string GetSha256Thumbprint(this X509Certificate cert)
        {
            byte[] buffer;
            using (SHA256 sha256 = SHA256.Create())
            {
                buffer = sha256.ComputeHash(cert.GetRawCertData());
            }
            string result = BitConverter.ToString(buffer).Replace("-", string.Empty);
            return result;
        }
    }
}
