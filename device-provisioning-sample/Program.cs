using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace DeviceProvisioningSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args == null || args.Length < 1) 
            {
                Console.WriteLine("You must specify an action!");
                return;
            }

            if (args[0] == "setup") 
            {
                CertificateFactory.CreateTestCert();
            }
            else
            {
                var scopeId = args[0];
                var device = new Device(scopeId);
                await device.Provision();
                await device.ConnectAndRun();
            }
        }
    }
}
