namespace GreeBlynkBridge.Gree
{
    using System.Collections.Generic;

    public class DeviceStatusChangedEventArgs
    {
        public Dictionary<string, int> Parameters { get; set; }
    }
}
