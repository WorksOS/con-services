using Newtonsoft.Json;

namespace ClientModel.DeviceConfig.Request.DeviceConfig.Switches
{
    public class DeviceConfigDualStateSwitchRequest: DeviceConfigSwitchesRequestBase
    {
        [JsonProperty("switchEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool SwitchEnabled { get; set; }
        [JsonProperty("switchOpen", NullValueHandling = NullValueHandling.Ignore)]
        public string SwitchOpen { get; set; }
        [JsonProperty("switchClosed", NullValueHandling = NullValueHandling.Ignore)]
        public string SwitchClosed { get; set; }
    }
}