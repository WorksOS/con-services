using CommonModel.AssetSettings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ClientModel.AssetSettings.Request
{
	public class AssetTargetSettings
    {
        [JsonProperty(Required = Required.Always, PropertyName = "assetTargetSettings", NullValueHandling = NullValueHandling.Ignore)]
        public List<AssetSettingsWeeklyTargets> assetTargetSettings { get; set; }

        public Guid? UserUID { get; set; }
        public Guid? CustomerUID { get; set; }
    }
}
