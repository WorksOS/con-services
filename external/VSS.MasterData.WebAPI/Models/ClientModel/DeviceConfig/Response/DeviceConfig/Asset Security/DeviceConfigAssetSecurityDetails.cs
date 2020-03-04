using Newtonsoft.Json;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.Asset_Security
{
	public class DeviceConfigAssetSecurityDetails : DeviceConfigResponseBase
    {
        [JsonProperty("securityMode", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SecurityMode { get; set; }
        [JsonProperty("securityStatus", NullValueHandling = NullValueHandling.Ignore)]
        public int? SecurityStatus { get; set; }
    }
}