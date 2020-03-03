using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.DeviceSettings
{
    public class DeviceConfigurationSettingsConfig
    {
        [JsonProperty("sendToDevice", NullValueHandling = NullValueHandling.Ignore)]
        public bool SendToDevice { get; set; }
        [JsonProperty("allowAdditionalTopic", NullValueHandling = NullValueHandling.Ignore)]
        public bool AllowAdditionalTopic { get; set; }
    }
}
