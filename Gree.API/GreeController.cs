using GreeBlynkBridge;
using GreeBlynkBridge.Gree;
using GreeBlynkBridge.VladControllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Controller = Microsoft.AspNetCore.Mvc.Controller;

namespace Gree.API
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GreeController : Controller
    {
        private readonly IConditionerService _conditionerService;

        public GreeController(IConditionerService conditionerService)
        {
            _conditionerService = conditionerService;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public IActionResult GetConditionersCount()
        {
            return Ok(_conditionerService.GetControllers().Count.ToString());
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> StopAllAsync()
        {
            foreach (var controller in _conditionerService.GetControllers())
            {
                await controller.SetDeviceParameter(DeviceParameterKeys.Power, 0);
            }
            return Ok();
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> CoolBedroomAsync()
        {
            await _conditionerService.GetController(ControllerEnum.Bedroom)
                .SetDeviceParameters(new List<Param>()
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

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> CoolKitchenAsync()
        {
            await _conditionerService.GetController(ControllerEnum.Kitchen).SetDeviceParameters(new List<Param>()
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

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetBedroomStatus()
        {
            var bedroomController = _conditionerService.GetController(ControllerEnum.Bedroom);

            await bedroomController.UpdateDeviceStatus();
            //TODO: Consider removing this line
            //string status = string.Join(string.Empty, bedroomController.Parameters.Select(a => $"{a.Key} = {a.Value}"));

            return Ok(bedroomController.Parameters);
        }
    }
}
