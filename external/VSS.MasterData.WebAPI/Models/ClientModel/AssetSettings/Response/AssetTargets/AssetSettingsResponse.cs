using CommonModel.AssetSettings;
using CommonModel.Enum;
using Newtonsoft.Json;
using System;

namespace ClientModel.AssetSettings.Response.AssetTargets
{
	public class AssetSettingsResponse : AssetSettingsBase
    {
        [JsonIgnore]
        public Guid AssetConfigUID { get; set; }

        [JsonProperty("targetValue", NullValueHandling = NullValueHandling.Ignore)]
        public double TargetValue { get; set; }

        [JsonIgnore]
        public AssetTargetType TargetType { get; set; }
    }
}
