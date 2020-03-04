using Newtonsoft.Json;

namespace ClientModel.DeviceConfig.Request.DeviceConfig.Switches
{
    public class DeviceConfigSingleStateSwitchRequest : DeviceConfigSwitchesRequestBase
    {
      [JsonProperty("switchActiveState", NullValueHandling = NullValueHandling.Ignore)]
      public string SwitchActiveState { get; set; }
      
    }
}
