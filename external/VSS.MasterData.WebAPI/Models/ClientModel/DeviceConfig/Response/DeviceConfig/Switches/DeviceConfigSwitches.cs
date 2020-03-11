using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.Switches
{
    public class DeviceConfigSwitches : DeviceConfigResponseBase
    {
        [JsonProperty("singleStateSwitches", NullValueHandling = NullValueHandling.Ignore)]
        public List<DeviceConfigSingleStateSwitch> SingleStateSwitch { get; set; }
        [JsonProperty("dualStateSwitches", NullValueHandling = NullValueHandling.Ignore)]
        public List<DeviceConfigDualStateSwitch> DualStateSwitch{ get; set; }
    }
}
