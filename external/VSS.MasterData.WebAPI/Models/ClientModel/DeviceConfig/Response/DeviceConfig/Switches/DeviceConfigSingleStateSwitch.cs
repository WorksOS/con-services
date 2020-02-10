using Newtonsoft.Json;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.Switches
{
    public class DeviceConfigSingleStateSwitch : DeviceConfigSwitchesBase
    {
        [JsonProperty("switchActiveState", NullValueHandling = NullValueHandling.Ignore)]
        public string SwitchActiveState { get; set; }
    }
}
