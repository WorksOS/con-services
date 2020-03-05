using Newtonsoft.Json;
using System;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.Switches
{
    public class DeviceConfigConfiguredDualStateSwitches
    {
        [JsonProperty("switchNumber", NullValueHandling = NullValueHandling.Ignore)]
        public int SwitchNumber { get; set; }
        [JsonProperty("switchName", NullValueHandling = NullValueHandling.Ignore)]
        public string SwitchName { get; set; }
        [JsonProperty("switchOpen", NullValueHandling = NullValueHandling.Ignore)]
        public string SwitchOpen { get; set; }
        [JsonProperty("switchClosed", NullValueHandling = NullValueHandling.Ignore)]
        public string SwitchClosed { get; set; }
        [JsonIgnore]
        public Guid? CustomerUID { get; set; }
        [JsonIgnore]
        public Guid? UserUID { get; set; }
        [JsonIgnore]
        public bool SwitchEnabled { get; set; }
    }
}
