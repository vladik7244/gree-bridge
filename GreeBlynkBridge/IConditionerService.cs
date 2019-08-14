using GreeBlynkBridge.Gree;
using GreeBlynkBridge.VladControllers;
using System.Collections.Generic;

namespace GreeBlynkBridge
{
    public interface IConditionerService
    {
        List<Controller> GetControllers();

        Controller GetController(ControllerEnum controller);
    }
}
