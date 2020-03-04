using CommonModel.AssetSettings;
using Newtonsoft.Json;
using System;

namespace ClientModel.AssetSettings.Response.AssetTargets
{
	public class AssetFuelBurnRateSettingsDetails : AssetSettingsBase
    {
        [JsonIgnore]
        public Guid AssetConfigUID { get; set; }

        [JsonProperty("idleTargetValue")]
        public double IdleBurnRateTargetValue { get; set; }

        [JsonProperty("workTargetValue")]
        public double WorkBurnRateTargetValue { get; set; }
    }
}
