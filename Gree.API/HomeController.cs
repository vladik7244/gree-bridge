using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreeBlynkBridge;
using GreeBlynkBridge.Gree;
using Microsoft.AspNetCore.Mvc;
using Controller = Microsoft.AspNetCore.Mvc.Controller;

namespace Gree.API
{
    class A
    {
        public string B {
            get;
            set;
        }
    }
    
    public class HomeController: Controller
    {
        public IActionResult GetConditionersCount()
        {
            return Ok(GreeBridge.GetControllers().Count.ToString());
        }

        public IActionResult StopAll()
        {
            foreach (var controller in GreeBridge.GetControllers())
            {
                controller.SetDeviceParameter(DeviceParameterKeys.Power, 0);
            }
            return Ok();
        }

        public IActionResult CoolBedroom()
        {
            GreeBridge.GetControllerByName("bed").SetDeviceParameters(new List<Param>()
            {
                new Param()
                {
                    name = DeviceParameterKeys.Power,
                    value = 1,
                },
                new Param()
                {
                    name = DeviceParameterKeys.FanSpeed,
                    value = 3,
                },
                new Param()
                {
                    name = DeviceParameterKeys.AirMode,
                    value = 0,
                },
                new Param()
                {
                    name = DeviceParameterKeys.SetTemperature,
                    value = 23,
                }
            });
            
            return Ok();
        }

        public IActionResult CoolKitchen()
        {
            GreeBridge.GetControllerByName("kit").SetDeviceParameters(new List<Param>()
            {
                new Param()
                {
                    name = DeviceParameterKeys.Power,
                    value = 1,
                },
                new Param()
                {
                    name = DeviceParameterKeys.FanSpeed,
                    value = 3,
                },
                new Param()
                {
                    name = DeviceParameterKeys.AirMode,
                    value = 0,
                },
                new Param()
                {
                    name = DeviceParameterKeys.SetTemperature,
                    value = 23,
                }
            });
            return Ok();
        }
        
        public async Task<IActionResult> GetStatus()
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            GreeBlynkBridge.Gree.Controller.DeviceStatusChangedEventHandler s = (object sender, DeviceStatusChangedEventArgs e) =>
            {
                tcs.TrySetResult(String.Join(String.Empty, e.Parameters.Select(a => $"{a.Key} = {a.Value}")));
            };
            GreeBridge.GetControllerByName("bed").DeviceStatusChanged += s;
            
            GreeBridge.GetControllerByName("bed").UpdateDeviceStatus();
            
            
            var result = await tcs.Task;

            GreeBridge.GetControllerByName("bed").DeviceStatusChanged -= s;

            return Ok(result);
        }
    }
}