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
            var ticks = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var rsa = RSA.Create(2048);
            var request = new CertificateRequest($"cn=globomantics-dev-{ticks}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1));

            // Create PFX (PKCS #12) with private key
            var keyPath = Path.GetFullPath("key.pfx");
            Console.WriteLine($"Writing private key to {keyPath}...");
            File.WriteAllBytes(keyPath, certificate.Export(X509ContentType.Pfx));

            // Create Base 64 encoded CER (public key only)
            var certPath = Path.GetFullPath("cert.cer");
            Console.WriteLine($"Writing public certificate to {certPath}...");
            File.WriteAllText(certPath, Convert.ToBase64String(certificate.Export(X509ContentType.Cert)));
        }
    }
}
