using Newtonsoft.Json;

namespace ClientModel.AssetSettings.Request.AssetSettings
{
	public class AssetFuelBurnRateSettingRequest: AssetSettingsRequestBase
    {
        [JsonProperty(Required = Required.Always, PropertyName = "idleTargetValue")]
        public double? IdleTargetValue { get; set; }

        [JsonProperty(Required = Required.Always, PropertyName = "workTargetValue")]
        public double? WorkTargetValue { get; set; }
    }
}
