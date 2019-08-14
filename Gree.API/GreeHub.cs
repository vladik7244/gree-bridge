using System.Collections.Generic;
using System.Threading.Tasks;
using GreeBlynkBridge;
using GreeBlynkBridge.Gree;
using GreeBlynkBridge.Logging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Gree.API
{
    public class GreeHub : Hub
    {
        private static ILogger log = Logger.CreateLogger("GreeHub");

        public string GetConditionersCount()
        {
            //await Clients.All.SendAsync("ReceiveMessage", message);

            return GreeBridge.GetControllers().Count.ToString();
        }
    }
}