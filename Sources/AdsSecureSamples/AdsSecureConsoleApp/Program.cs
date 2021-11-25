#define TEST

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


namespace SimpleAdsSecureConsoleApp
{
    public enum TlsType
    {
        SelfCert,
        CA
    }

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
MIIDNzCCAh8CFDFkqPhIY5TR1kSDldjD6YgIpBcpMA0GCSqGSIb3DQEBCwUAMFgx
CzAJBgNVBAYTAkRFMQwwCgYDVQQIDANOUlcxDTALBgNVBAcMBFZlcmwxCzAJBgNV
BAoMAkJrMQ0wCwYDVQQLDARUQ1BNMRAwDgYDVQQDDAdSQUxGSDA0MB4XDTIxMTEy
NTExMTQxMFoXDTIyMTEyMDExMTQxMFowWDELMAkGA1UEBhMCREUxDDAKBgNVBAgM
A05SVzENMAsGA1UEBwwEVmVybDELMAkGA1UECgwCQmsxDTALBgNVBAsMBFRDUE0x
EDAOBgNVBAMMB1JBTEZIMDQwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIB
AQDgsQQw0dlZxEtqmV27JppGEchMq2hj/o8eFPta08k126AW+miKpwnuZ1tfBp9N
90+/VcVaEl3KermWpiyTcdLegVodvRqcj+K+qnoYyUE5uPDbDUQhRL8F+3nLbfiZ
Ml1iwCVpwtY0eEh6lSgDU55zHTZstoQXdUpvkDlouj9GTo+Avip8m14wXKrlx/aQ
uv9TLBioQJmzC7rXcuWJtBJg5PRzpbkKW9sONazEV7hY69i4igcMtzNIAYk+CZJq
8x/SLM4RH+14MkagPnk/a7sbFzKaVvLBLLbTZZO/eb7BGXO98xpRv4ZIagEluQpX
0D8MsQm83Sz6QOqBaAayqWaZAgMBAAEwDQYJKoZIhvcNAQELBQADggEBAMS9SlDv
vtjKkjfXECj17yev3GkL7b1KAdqy+ISnEYNBHl79SUqXVJu0rzebAcqrZR/UcuTm
aAhX/qHGB0GOcfq3pEp36QEft82+I92tUjs4bA5OAIdQiyZ58RNi2PULullTNLjq
nYY4yPh6pSqHwpBgXPK+ZkgRJiZrc3uzrJFzEZNxjHul9Hok2VabjhD7MI1os/rx
DiF2I+mdl9jVCev5E7qWEg4OOheTz01Hy9tCisK5s3qPFHKcZxftzG8LwFunTSQH
n/3B+5H3prYbmyOVpq+sMIhVUD0FcsTeJ++9uWNnh26njztG8eEvTEe06en9MLdc
EU1wxU9DS36L3PQ=
-----END CERTIFICATE-----";

        /// <summary>
        /// RootCA certificate (PEM-Format, copied from .\certs\RootCA.pem)
        /// </summary>
        static string s_ca = @"-----BEGIN CERTIFICATE-----
MIIDkTCCAnmgAwIBAgIUVMnOTt6UQ8fsn69ye5W/fgLMk/UwDQYJKoZIhvcNAQEL
BQAwWDELMAkGA1UEBhMCREUxDDAKBgNVBAgMA05SVzENMAsGA1UEBwwEVmVybDEL
MAkGA1UECgwCQmsxDTALBgNVBAsMBFRDUE0xEDAOBgNVBAMMB1JBTEZIMDQwHhcN
MjExMTI1MTExMjA2WhcNMzExMDA0MTExMjA2WjBYMQswCQYDVQQGEwJERTEMMAoG
A1UECAwDTlJXMQ0wCwYDVQQHDARWZXJsMQswCQYDVQQKDAJCazENMAsGA1UECwwE
VENQTTEQMA4GA1UEAwwHUkFMRkgwNDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCC
AQoCggEBAMVVAdKfFYTLgnJWLoU8V40NmmcGEWoA34Y86JcPJSykzlKmCnyTOrQl
6A8m/JWnJNMp8GBgpBgtXzSQGVW9Nz4c2PiipHXVfvmqAk7DxwfjoyGc3zK3Sq3o
uXOOLj2rna04ylYSJor7BgIdrWV60lOQXOirO6cd5+dGffYGBTtvbuKADbQvoUPK
wl64Px8yGgtUB+Y4RWzgidUTXTT508eNflBIAKFvvaju0e1OaDd15EVV90KRD7eU
J48jxdP3LwO2ha5RBdmZtx8drLCtAW96QUD0pgUKYL9woZoAOh2yjmgaVom4llOd
V//riUNWrKB490YdYv2dQnXNM3wQYOsCAwEAAaNTMFEwHQYDVR0OBBYEFH52o2q3
MK9aIxDehGvy6I7ZdtyzMB8GA1UdIwQYMBaAFH52o2q3MK9aIxDehGvy6I7Zdtyz
MA8GA1UdEwEB/wQFMAMBAf8wDQYJKoZIhvcNAQELBQADggEBAC2AfP3JnhnPpaXI
dsoPTrRMlkDWHWTe6psQedbzJWkTRyKBktTDnj8cVX6Vvjkiw/IfXgPiJun0cGKv
kgRk1AYzUs0rGa3Q7jxb6WeN/vcuzgLxJ8Lue/dCs6zSAxtw/TKaZtTpyvY9CurF
Q4SUUew74x6i0gnMGVTPtOL9cVuWa+eflxwGMYGqyakYjbDK/fPv+fRu/81Wz1+z
S9I3Y3xrmWE3p184scgOxMzzQdN1c2gyqtf1v3aWgEYK3kVP+IOeDbCXhGTBq1/1
KU74Jx5x7NqOLE3Y3mCyJzSHrZX6M8zKGhjJhlAYvlpXHByGdpxhIWB+q9NgOsnn
SeJPHkU=
-----END CERTIFICATE-----";

        /// <summary>
        /// RSA Private key as string (PEM format, copied from .\certs\SourceIPC.key)
        /// </summary>
        static string s_rsaPrivateKey = @"-----BEGIN RSA PRIVATE KEY-----
MIIEowIBAAKCAQEA4LEEMNHZWcRLaplduyaaRhHITKtoY/6PHhT7WtPJNdugFvpo
iqcJ7mdbXwafTfdPv1XFWhJdynq5lqYsk3HS3oFaHb0anI/ivqp6GMlBObjw2w1E
IUS/Bft5y234mTJdYsAlacLWNHhIepUoA1Oecx02bLaEF3VKb5A5aLo/Rk6PgL4q
fJteMFyq5cf2kLr/UywYqECZswu613LlibQSYOT0c6W5ClvbDjWsxFe4WOvYuIoH
DLczSAGJPgmSavMf0izOER/teDJGoD55P2u7GxcymlbywSy202WTv3m+wRlzvfMa
Ub+GSGoBJbkKV9A/DLEJvN0s+kDqgWgGsqlmmQIDAQABAoIBAAPLmamFceltQmTb
kFS/8y0p0btRzeKagypb6l7/Ys+xFQQuWKO27JkFT+rL/tbfTY6K0VmTI9huuEHO
LsSS0KAHiVElR5TLbWYRFRzkEWD7Ob5JfBQfyCY+uDDye5eC3Ub0ve0KaHncakT/
zfw4Zl3FaYzujE6lIYu+4Ole9ACo/PIfudXKk1tJEBLrcgIrs262SYFImrWAWx+0
NMuRgsYYYYUUI79/Wpgiaxg+Stg4wUtwDUA+RYAcujp6hvn0Kg8/82jmCVoZlg1R
AcPR1YVW2ZR/hPpgbSEx3zvUuoBsVK0xtqGIbdo9gOnEd7uu8tQn2vm4BwoNLujN
DiWd8d0CgYEA+Dgk+grdrENmdZThmWyE3MhbMCHXONMpvtCPx28ZGvtmIc59PTcC
Pq4cWX2Q8w/WnbVfv/I6GDMie2/E9SNRbn55ozQ3HmiVykr0VhmMNExabaCLH8nQ
ElVAbrVZIHxGzzn6/1LlVBXHxsbl2fhYvBkKN0z7G+s0N0OcmcBN4CcCgYEA57wS
JTlzbYGP6QF1hxZnQL3B05R+MXFT2VMgDbylEAArbbYeWTidz161Xq+tT+uEWFZW
Sc9w6JQ9jwetCWVuWdEJvFuIx6EV3Bh0q2XK3bg4R1LOfTHGq/sF03mZ81X5eHSd
uZR1+SdzepjINETyMJnEKUGhmizUg5s65yAf+z8CgYEAybh29G2YMMKlpbDUlmbG
otaApOEbkyaoqlW8Qwtaj773BUpWJUVrIZ1FlMSi46VfeNNJeShVZg1IXKA8pCuL
pgxKtgNdN+0urjOz1bT1aBsU8jqiVbcgzYVS06W1RN06fnZUMOMhU/BPZo+FhFp7
YoHG96IsAEhpKvBbd+f8YckCgYB4DG8eB6Arh6Yk4FOhUtLUsDkcQd4KARqeCDkf
xK2CF4RoBqO8Nt9SEU9GKR5Qu8LI/JkhDa0BX+JwGVrj9j7vmqI/iO/X8zRe2/B8
5nPs3sWQ9W3xX3r7l0RSZLmDXPOrGkanYCiplW12gnWc1mbdFJuRf+WW+EhzkVQ3
beYDgwKBgGCFgiCRaKLtqYkHcTolNLapPKmfOi4vhNHPSRuHeiK9Qk1zseYL7MGO
hyybbJB0RgxX7ofiWItbJh9I06ENOBcvjyCReyLoJDoSaqsj0upxfpu2GiDxxRo/
6+KQclfKaiQvD6kJnVT+zvZ/Gupx/TZcJu3/+uvToItGVvvftjoQ
-----END RSA PRIVATE KEY-----";

        static async Task Main(string[] args)
        {
            bool selfSigned = true;

            CancellationToken cancel = CancellationToken.None;

            bool ok = false;
            AmsNetId targetNetId;

            do
            {
                Console.Write("Enter Target NetId: ");
                string targetNetIdStr = Console.ReadLine();
                
                ok = AmsNetId.TryParse(targetNetIdStr, out targetNetId);
            } while (!ok);

            int targetPort = 0;

            do
            {
                Console.Write("Enter Target AdsPort: ");
                string targetPortStr = Console.ReadLine();
                ok = int.TryParse(targetPortStr, out targetPort);
            } while (!ok);

            
            AmsAddress targetAddress = new AmsAddress(targetNetId, targetPort);

            string hostOrIp;

            do
            {
                Console.Write("Enter Target HostName/IPAddress: ");
                hostOrIp = Console.ReadLine();
            } while (string.IsNullOrEmpty(hostOrIp));



            string? userName = null;
            SecureString securePwd = new SecureString();

            if (selfSigned)
            {
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
                        securePwd.AppendChar(key.KeyChar);
                        Console.Write("*");
                    }
                    // Exit if Enter key is pressed.
                } while (key.Key != ConsoleKey.Enter);
                Console.WriteLine();
            }

            X509Certificate2 caCert = X509CertificateHelper.CreateFromPem(s_ca);
            //CAUTION: The TlsConnect, X509CertificateHelper, AdsReadStateResponseHeader objects are preliminary
            //and are actually not documented. They could be changed in future!

            // Loading Cert from PEM Formatted strings:
            X509Certificate2 certFromStrings = X509CertificateHelper.CreateFromPem(s_certificate, s_rsaPrivateKey);

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

            TlsConnect connect = null;
            try
            {
                if (selfSigned)
                {
                    connect = new TlsConnectSelfSigned(hostOrIp, certFromStrings, userName, securePwd, s_localNetId);
                }
                else
                {
                    connect = new TlsConnectCA(hostOrIp, caCert, certFromStrings, s_localNetId);
                }
                await connect.ConnectAsync(targetAddress, cancel);

                SslStream sslStream = connect.GetStream();

                string serverHostName = sslStream.TargetHostName;
                string serverSubject = sslStream.RemoteCertificate.Subject;
                string serverIssuer = sslStream.RemoteCertificate.Issuer;
                //string sha1Thumbprint = sslStream.RemoteCertificate.GetCertHashString();
                string sha256FingerPrint = sslStream.RemoteCertificate.GetSha256Thumbprint();

                Console.WriteLine($"Connected to Server '{serverHostName}' (Subject: {serverSubject}, Issuer: {serverIssuer}");

                bool trustTarget = false;

                if (selfSigned)
                {
                    //Ensure that the Server/Target Fingerprint is correct.
                    //We have to compare the Fingerprint to trust the AdsServer/Target
                    Console.WriteLine($"Fingerprint: {sha256FingerPrint}");
                    Console.WriteLine($"Is the Server/Target Fingerprint correct (y/n)?");
                    ConsoleKeyInfo consoleKey = Console.ReadKey();

                    if (consoleKey.KeyChar == 'y' || consoleKey.KeyChar == 'Y')
                    {
                        trustTarget = true;
                    }
                }
                else
                {
                    // CA Certification is safe/trusted in itself (both communication ends are certified by the same RootCA)
                    trustTarget = true;
                }

                if (trustTarget)
                {
                    AdsState state = await ReadStateAsync(connect, CancellationToken.None);

                    Console.WriteLine("");
                    Console.WriteLine($"SUCCESS: The Ads state of target {targetAddress} is '{state}'.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
            connect.Close();
        }

        static async Task<AdsState> ReadStateAsync(TlsConnect connection, CancellationToken cancel)
        {
            //CAUTION: The TlsConnect, X509CertificateHelper, AdsReadStateResponseHeader objects are preliminary
            //and are actually not documented. They could be changed in future!

            AdsState state = AdsState.Invalid;
            AmsHeader header = new AmsHeader(connection.TargetAddress, connection.LocalAddress, AdsCommandId.ReadState, AmsStateFlags.AdsCommand, 0, 0, 1);

            AmsCommand command = new AmsCommand(header, Array.Empty<byte>());
            int size = AmsCommandFrameMarshaller.MarshalSize(command);

            byte[] requestBuffer = new byte[size];
            int marshalledBytes = AmsCommandFrameMarshaller.Marshal(command, requestBuffer.AsSpan());

            SslStream securedStream = connection.GetStream();
            await securedStream.WriteAsync(requestBuffer, cancel).ConfigureAwait(false);

            int length = AmsHeader.MarshalSize + AdsReadStateResponseHeader.StaticMarshalSize;
            byte[] responseBuffer = new byte[length];
            int returnedBytes = await securedStream.ReadAsync(responseBuffer, cancel).ConfigureAwait(false);

            AmsHeader responseHeader;
            int offset = AmsHeaderMarshaller.Unmarshal(responseBuffer.AsSpan(0), out responseHeader);

            AdsErrorCode adsError = (AdsErrorCode)BinaryPrimitives.ReadUInt32LittleEndian(responseBuffer.AsSpan(offset, 4));
            offset += 4;
            state = (AdsState)BinaryPrimitives.ReadUInt16LittleEndian(responseBuffer.AsSpan(offset, 2));
            offset += 2;
            ushort deviceState = BinaryPrimitives.ReadUInt16LittleEndian(responseBuffer.AsSpan(offset, 2));
            return state;
        }
    }

    public static class X509CertificateExtension
    {
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
