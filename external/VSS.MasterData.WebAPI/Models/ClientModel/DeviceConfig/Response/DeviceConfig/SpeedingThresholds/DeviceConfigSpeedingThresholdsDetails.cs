using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.SpeedingThresholds
{
    public class DeviceConfigSpeedingThresholdsDetails : DeviceConfigResponseBase
    {
        [JsonProperty("speedThreshold", NullValueHandling = NullValueHandling.Ignore)]
        public int? SpeedThreshold { get; set; }
        [JsonProperty("speedThresholdEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SpeedThresholdEnabled { get; set; }
        [JsonProperty("speedThresholdDuration", NullValueHandling = NullValueHandling.Ignore)]
        public int? SpeedThresholdDuration { get; set; }
    }
}
