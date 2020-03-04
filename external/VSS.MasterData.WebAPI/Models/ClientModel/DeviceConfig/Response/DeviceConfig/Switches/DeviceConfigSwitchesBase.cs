using Newtonsoft.Json;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.Switches
{
    public class DeviceConfigSwitchesBase
    {
        [JsonProperty("switchNumber", NullValueHandling = NullValueHandling.Ignore)]
        public int SwitchNumber { get; set; }
        [JsonProperty("switchName", NullValueHandling = NullValueHandling.Ignore)]
        public string SwitchName { get; set; }
        [JsonProperty("switchSensitivity", NullValueHandling = NullValueHandling.Ignore)]
        public float SwitchSensitivity{get;set;}
        [JsonProperty("switchMonitoringStatus", NullValueHandling = NullValueHandling.Ignore)]
        public string MonitoredWhen { get; set; }
        [JsonProperty("switchParameterName", NullValueHandling = NullValueHandling.Ignore)]
        public string SwitchParameterName { get; set; }
    }
}
