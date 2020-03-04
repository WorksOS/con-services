using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.MovingThreshold
{
    public class DeviceConfigMovingThresholdDetails : DeviceConfigResponseBase
    {
        [JsonProperty("radius", NullValueHandling = NullValueHandling.Ignore)]
        public int? Radius { get; set; }
        [JsonProperty("movingOrStoppedThreshold", NullValueHandling = NullValueHandling.Ignore)]
        public double? MovingOrStoppedThreshold { get; set; }
        [JsonProperty("movingThresholdsDuration", NullValueHandling = NullValueHandling.Ignore)]
        public int? MovingThresholdsDuration { get; set; }
    }
}
