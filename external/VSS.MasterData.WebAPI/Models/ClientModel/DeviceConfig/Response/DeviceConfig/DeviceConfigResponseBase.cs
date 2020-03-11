using Newtonsoft.Json;
using System;

namespace ClientModel.DeviceConfig.Response.DeviceConfig
{
    public class DeviceConfigResponseBase
    {
        [JsonProperty("assetUID", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid? AssetUID { get; set; }

        [JsonProperty("deviceUID", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid? DeviceUID { get; set; }

        [JsonProperty("deviceType", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string DeviceType { get; set; }

        [JsonProperty("lastUpdatedOn", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime LastUpdatedOn { get; set; }
    }
}
