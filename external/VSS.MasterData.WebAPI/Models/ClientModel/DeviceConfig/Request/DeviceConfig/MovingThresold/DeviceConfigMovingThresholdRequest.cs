using Newtonsoft.Json;
using Infrastructure.Common.DeviceSettings.Enums;

namespace ClientModel.DeviceConfig.Request.DeviceConfig.MovingThresold
{
    public class DeviceConfigMovingThresholdRequest : DeviceConfigRequestBase
    {
        [JsonProperty("radius", NullValueHandling = NullValueHandling.Ignore)]
        public int? Radius { get; set; }
        [JsonProperty("movingOrStoppedThreshold", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? MovingOrStoppedThreshold { get; set; }
        [JsonProperty("movingThresholdsDuration", NullValueHandling = NullValueHandling.Ignore)]
        public int? MovingThresholdsDuration { get; set; }
    }
}
