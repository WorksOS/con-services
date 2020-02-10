using Newtonsoft.Json;
using Infrastructure.Common.DeviceSettings.Enums;

namespace ClientModel.DeviceConfig.Request.DeviceConfig.SpeedingThresholds
{
    public class DeviceConfigSpeedingThresholdsRequest : DeviceConfigRequestBase
    {
        [JsonProperty("speedThreshold", NullValueHandling = NullValueHandling.Ignore)]
        public int? SpeedThreshold { get; set; }
        [JsonProperty("speedThresholdEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SpeedThresholdEnabled { get; set; }
        [JsonProperty("speedThresholdDuration", NullValueHandling = NullValueHandling.Ignore)]
        public int? SpeedThresholdDuration { get; set; }
    }
}
