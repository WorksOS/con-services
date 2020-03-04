using Newtonsoft.Json;

namespace ClientModel.AssetSettings.Request
{
	public class AssetSettingsRequest : AssetSettingsRequestBase
    {
        [JsonProperty(Required =Required.Always, PropertyName ="targetValue")]
        public double TargetValue { get; set; }
    }
}
