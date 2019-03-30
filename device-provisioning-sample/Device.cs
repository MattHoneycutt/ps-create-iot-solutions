using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Client;

namespace DeviceProvisioningSample
{
    public class Device
    {
        private string scopeId;
        private string assignedHub;
        private DeviceAuthenticationWithX509Certificate auth;

        public Device(string scopeId)
        {
            this.scopeId = scopeId;
        }

        public async Task Provision()
        {
            var certificate = LoadPrivateKey("key.pfx");

            using (var securityProvider = new SecurityProviderX509Certificate(certificate))
            using (var transport = new ProvisioningTransportHandlerAmqp(TransportFallbackType.TcpOnly))
            {
                var client = ProvisioningDeviceClient.Create("global.azure-devices-provisioning.net", this.scopeId, securityProvider, transport);

                var result = await client.RegisterAsync();

                Console.WriteLine($"Provisioning result: {result.Status}");

                if (result.Status != ProvisioningRegistrationStatusType.Assigned) 
                {
                    throw new InvalidOperationException("Something went wrong while trying to provision.");
                }

                this.assignedHub = result.AssignedHub;
                this.auth = new DeviceAuthenticationWithX509Certificate(result.DeviceId, securityProvider.GetAuthenticationCertificate());
            }
        }

        private static X509Certificate2 LoadPrivateKey(string key)
        {
            if (!File.Exists(key)) 
            {
                Console.WriteLine("No private key found.  Execute `dotnet run setup` first.");
                Environment.Exit(-1);
            }

            var certificateCollection = new X509Certificate2Collection();
            certificateCollection.Import(key);

            foreach (var element in certificateCollection)
            {
                if (element.HasPrivateKey)
                {
                    return element;
                }
                else
                {
                    element.Dispose();
                }
            }

            throw new InvalidOperationException("No private key found.  Execute `dotnet run setup` first.");
        }

        internal async Task ConnectAndRun()
        {
            using (var client = DeviceClient.Create(this.assignedHub, this.auth, TransportType.Amqp))
            {
                Console.Write($"Connecting to hub: {this.assignedHub}... ");
                await client.OpenAsync();
                Console.WriteLine("Connected!");

                Console.Write("Sending D2C message... ");
                await client.SendEventAsync(new Message(Encoding.UTF8.GetBytes("Hello from a provisioned device!")));
                Console.WriteLine("Sent!");
                
                Console.Write("Closing connection... ");
                await client.CloseAsync();
                Console.WriteLine("Connection Closed!");
            }
        }
    }
}