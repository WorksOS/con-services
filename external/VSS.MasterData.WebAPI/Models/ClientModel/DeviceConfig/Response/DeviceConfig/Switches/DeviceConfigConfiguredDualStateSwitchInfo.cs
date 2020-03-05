using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.Switches
{
    public class DeviceConfigConfiguredDualStateSwitchInfo : DeviceConfigResponseBase
    {
        [JsonProperty("switches", NullValueHandling = NullValueHandling.Ignore)]
        public List<DeviceConfigConfiguredDualStateSwitches> Switches { get; set; }
    }
}
