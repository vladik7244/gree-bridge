using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using GreeBlynkBridge.Gree;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GreeBlynkBridge
{
    public class GreeBridge
    {
        private static ILogger log = Logging.Logger.CreateDefaultLogger();

        private static List<Controller> _controllers;

        public static List<Controller> GetControllers()
        {
            return _controllers;
        }
            
        public static Controller GetControllerByName(string name)
        {
            switch (name)
            {
                case "kit":
                    return _controllers.Single(c => c.DeviceID == "f4911e75f8c3");
                case "cab":
                    return _controllers.Single(c => c.DeviceID == "f4911e764fe0");
                case "bed":
                    return _controllers.Single(c => c.DeviceID == "f4911e75fa22");
                default:
                    throw new Exception("Name is wrong");
            }
        }

        public static async void Run()
        {
            var basePath = Directory.GetParent(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath).FullName;

            log.LogDebug($"basePath: {basePath}");

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("config.json");

            IConfiguration config;

            try
            {
                config = configBuilder.Build();
            }
            catch (Exception e)
            {
                log.LogCritical($"Failed to load configuration: {e.Message}");
                throw;
            }

            if (config["skip-initial-scan"] != "True")
            {
                await DiscoverDevices(config);
            }
            else
            {
                log.LogInformation("Skipping initial scan");
            }

            _controllers = Database.AirConditionerManager.LoadAll().Select((m) => new Gree.Controller(m)).ToList();
            log.LogDebug($"Controllers loaded: {_controllers.Count}");


            foreach (var c in _controllers)
            {
                c.DeviceStatusChanged += DeviceStatusChanged;
            }

            var deviceUpdateTimer = new Timer(10000)
            {
                Enabled = true,
                AutoReset = true
            };

            deviceUpdateTimer.Elapsed += async (o, e) =>
            {
                log.LogDebug("Updating device status");

                foreach (var controller in _controllers)
                {
                    await controller.UpdateDeviceStatus();
                }
            };
        }

        private static void DeviceStatusChanged(object sender, Gree.DeviceStatusChangedEventArgs e)
        {
            log.LogDebug($"Device ({(sender as Gree.Controller).DeviceName}) changed");
        }

        private static async Task DiscoverDevices(IConfiguration config)
        {
            var configEnum = config.AsEnumerable();

            var broadcastAddresses = configEnum.Where((e) => e.Key.StartsWith("network:broadcast:"));
            if (broadcastAddresses.Count() == 0)
            {
                log.LogCritical("No network broadcast addresses configured");
                return;
            }

            var foundUnits = new List<Database.AirConditionerModel>();

            foreach (var entry in broadcastAddresses)
            {
                log.LogInformation($"Scanning local devices using address {entry.Value}");

                foundUnits.AddRange(await Gree.Scanner.Scan(entry.Value));
            }

            log.LogInformation("Updating the database");

            foreach (var unit in foundUnits.Distinct(new Database.AirConditionerModelEqualityComparer()))
            {
                await Database.AirConditionerManager.UpdateAsync(unit);
            }
        }
    }
}