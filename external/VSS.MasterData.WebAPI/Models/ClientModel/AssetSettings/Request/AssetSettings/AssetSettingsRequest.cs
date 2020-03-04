using Newtonsoft.Json;

namespace ClientModel.AssetSettings.Request.AssetSettings
{
	public class AssetSettingsRequest : AssetSettingsRequestBase
    {
        [JsonProperty(Required = Required.Always, PropertyName = "targetValue")]
        public virtual double TargetValue { get; set; }
    }
}
