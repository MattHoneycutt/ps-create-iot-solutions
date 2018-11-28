using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace DeviceProvisioningSample
{
    public static class CertificateFactory
    {
        public static void CreateTestCert()
        {
            var ecdsa = ECDsa.Create(); 
            var request = new CertificateRequest("cn=globomantics-prov-dev-01", ecdsa, HashAlgorithmName.SHA256);
            var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1));

            // Create PFX (PKCS #12) with private key
            Console.WriteLine("Writing private key to key.pfx...");
            File.WriteAllBytes("key.pfx", certificate.Export(X509ContentType.Pfx));

            // Create Base 64 encoded CER (public key only)
            Console.WriteLine("Writing public certificate to cert.cer...");
            File.WriteAllText("cert.cer", "-----BEGIN CERTIFICATE-----\r\n"
                + Convert.ToBase64String(certificate.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks)
                + "\r\n-----END CERTIFICATE-----");
    
        }

    }
}
