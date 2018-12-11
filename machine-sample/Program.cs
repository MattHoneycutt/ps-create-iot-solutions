using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace MachineSample
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

            Console.WriteLine("Starting simulated machine, press enter to exit!");
            RunSimulatedMachine(device);
            Console.ReadLine();
        }

        private static async Task RunSimulatedMachine(DeviceClient device)
        {
            var rand = new Random();

            var startTime = DateTime.Now.AddMinutes(1);
            while (DateTime.Now < startTime) 
            {
                await SendStatus(device,
                                 temperature: RandomBetween(rand, 70, 71),
                                 oilPressure: 0,
                                 coolantLevel: 75,
                                 vibrationLevel: 0);

                Thread.Sleep(1000);
            }

            //var eventTime = DateTime.Now.AddMinutes(10);
            var eventTime = DateTime.Now.AddMinutes(2);
            while (DateTime.Now < eventTime) 
            {
                await SendStatus(device,
                    temperature: RandomBetween(rand, 125, 127),
                    oilPressure: RandomBetween(rand, 50, 55),
                    coolantLevel: 75,
                    vibrationLevel: RandomBetween(rand, 25, 30));

                Thread.Sleep(1000);
            }

            //The BANG
            await SendStatus(device,
                temperature: RandomBetween(rand, 125, 127),
                oilPressure: RandomBetween(rand, 50, 55),
                coolantLevel: 75,
                vibrationLevel: 100);

            Thread.Sleep(1000);

            var catastrophicEventTime = DateTime.Now.AddMinutes(2);
            var temperature = 127m;
            var oilPressure = 55m; 
            var coolantLevel = 75m;
            var vibrationLevel = 50m;
            while (DateTime.Now < catastrophicEventTime) 
            {
                coolantLevel = RandomBetween(rand, coolantLevel-2, coolantLevel);

                if (coolantLevel < 50)
                {
                    temperature = RandomBetween(rand, temperature, temperature+5);
                    oilPressure = RandomBetween(rand, oilPressure, oilPressure+5);
                    vibrationLevel = RandomBetween(rand, vibrationLevel, vibrationLevel+2);
                }
                else 
                {
                    temperature = RandomBetween(rand, 125, 127);
                    oilPressure = RandomBetween(rand, 50, 55);
                    vibrationLevel = RandomBetween(rand, 40, 50);
                }

                await SendStatus(device,
                    temperature,
                    oilPressure,
                    coolantLevel,
                    vibrationLevel);

                Thread.Sleep(1000);
            }

            DrawExplosion();
        }

        private static async Task SendStatus(DeviceClient device, decimal temperature, decimal oilPressure, decimal coolantLevel, decimal vibrationLevel)
        {
            Console.Write(".");
            await SendMessageTo(device, new TelemetryMessage 
            {
                SensorName = "Temperature",
                Value = temperature
            });
            await SendMessageTo(device, new TelemetryMessage 
            {
                SensorName = "OilPressure",
                Value = oilPressure
            });
            await SendMessageTo(device, new TelemetryMessage 
            {
                SensorName = "CoolantLevel",
                Value = coolantLevel
            });
            await SendMessageTo(device, new TelemetryMessage 
            {
                SensorName = "VibrationLevel",
                Value = vibrationLevel
            });
        }

        private static void DrawExplosion()
        {
            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("Bob: Hey, should it be smoking like that?");
            Console.WriteLine("Steve: No, probably not...");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine(@"
     _.-^^---....,,--       
 _--                  --_  
<        BOOOOOOM        >)
|                         | 
 \._                   _./  
    ```--. . , ; .--'''       
          | |   |             
       .-=||  | |=-.   
       `-=*******=-'   
          | ;  :|     
 _____.,-----------,._____
");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("(no virtual beings were harmed in this catastrophic event)");
        }

        private static decimal RandomBetween(Random rand, decimal lowValue, decimal highValue)
        {
            return lowValue + (decimal)rand.NextDouble() * (highValue - lowValue);
        }

        private static async Task SendMessageTo(DeviceClient device, TelemetryMessage telemetry)
        {
            var json = JsonConvert.SerializeObject(telemetry);
            var message = new Message(Encoding.ASCII.GetBytes(json));

            await device.SendEventAsync(message);
        }
    }
}
