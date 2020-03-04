using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.Switches
{
    public class DeviceConfigConfiguredDualStateSwitchConfig
    {
        [JsonProperty("switchState", NullValueHandling = NullValueHandling.Ignore)]
        public bool SwitchEnabled { get; set; }
        [JsonProperty("switchOpen", NullValueHandling = NullValueHandling.Ignore)]
        public string SwitchOpen { get; set; }
        [JsonProperty("switchClose", NullValueHandling = NullValueHandling.Ignore)]
        public string SwitchClosed { get; set; }
    }
}
