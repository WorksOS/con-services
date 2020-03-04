using Newtonsoft.Json;
using Infrastructure.Common.DeviceSettings.Enums;

namespace ClientModel.DeviceConfig.Request.DeviceConfig.Switches
{
    public class DeviceConfigSwitchesRequestBase
    {
        [JsonProperty("switchName", NullValueHandling = NullValueHandling.Ignore)]
        public string SwitchName { get; set; }
        [JsonProperty("switchSensitivity", NullValueHandling = NullValueHandling.Ignore)]
        public float SwitchSensitivity { get; set; }
        [JsonProperty("switchParameterName", NullValueHandling = NullValueHandling.Ignore)]
        public string SwitchParameterName { get; set; }
        [JsonProperty("switchMonitoringStatus", NullValueHandling = NullValueHandling.Ignore)]
        public string SwitchMonitoringStatus { get; set; }
        [JsonProperty("switchNumber", NullValueHandling = NullValueHandling.Ignore)]
        public int SwitchNumber { get; set; }
    }
}
