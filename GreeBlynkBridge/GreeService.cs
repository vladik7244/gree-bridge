using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using GreeBlynkBridge.Gree;
using GreeBlynkBridge.VladControllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GreeBlynkBridge
{
    public class GreeService : IConditionerService
    {
        private readonly ILogger log = Logging.Logger.CreateDefaultLogger();

        private List<Controller> _controllers;

        private static readonly Dictionary<ControllerEnum, string> _controllersMap
            = new Dictionary<ControllerEnum, string>
            {
                {ControllerEnum.Kitchen, "f4911e75f8c3"},
                {ControllerEnum.Bedroom, "f4911e75fa22"},
                {ControllerEnum.SecondaryBedroom, "f4911e764fe0"}
            };

        public GreeService(IConfiguration config)
        {
            this.Initialize(config);
        }

        public List<Controller> GetControllers()
        {
            return _controllers;
        }

        public Controller GetController(ControllerEnum location)
        {
            return _controllersMap.TryGetValue(location, out string id)
                ? _controllers.Single(c => c.DeviceID == id)
                : throw new KeyNotFoundException($"Controller not found: {location.ToString()}");
        }

        private async void Initialize(IConfiguration config)
        {
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
                c.DeviceStatusChanged += (sender, e) =>
                {
                    log.LogDebug($"Device ({(sender as Gree.Controller).DeviceName}) changed");
                };
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

        private async Task DiscoverDevices(IConfiguration config)
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
