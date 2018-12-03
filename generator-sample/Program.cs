using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace GeneratorSample
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length < 1) 
            {
                Console.WriteLine("You must specify a device connection string!");
                return;
            }

            var device = DeviceClient.CreateFromConnectionString(args[0]);

            Console.WriteLine("Starting simulated generator, press enter to exit!");
            RunSimulatedGeneratorDataAsync(device);
            Console.ReadLine();
        }

        private static async System.Threading.Tasks.Task RunSimulatedGeneratorDataAsync(DeviceClient device)
        {
            var rand = new Random();

            var nextAnomalousEvent = DateTime.Now.AddSeconds(30);

            while (true) 
            {
                var temperature = new TelemetryMessage 
                {
                    SensorName = "Temperature",
                    Value = rand.Next(50, 100)
                };

                var rpms = new TelemetryMessage 
                {
                    SensorName = "RPMs",
                    Value = rand.Next(500, 600)
                };

                if (nextAnomalousEvent < DateTime.Now) 
                {
                    Console.WriteLine("**GENERATING ANOMALY!");
                    temperature.Value = rand.Next(200, 250);
                    rpms.Value = rand.Next(750, 900);
                    nextAnomalousEvent = DateTime.Now.AddSeconds(30);
                }
                else 
                {
                    Console.WriteLine("Sending normal message...");
                }

                await SendMessageTo(device, temperature);
                await SendMessageTo(device, rpms);

                Thread.Sleep(1000);
            }
        }

        private static async Task SendMessageTo(DeviceClient device, TelemetryMessage telemetry)
        {
            var json = JsonConvert.SerializeObject(telemetry);
            var message = new Message(Encoding.ASCII.GetBytes(json));

            await device.SendEventAsync(message);
        }
    }
}
