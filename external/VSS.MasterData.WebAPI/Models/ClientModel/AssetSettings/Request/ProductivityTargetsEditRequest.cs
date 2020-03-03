using CommonModel.AssetSettings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ClientModel.AssetSettings.Request
{
	public class AssetProductivityTargets
    {
        [JsonProperty("assetProductivitySettings", NullValueHandling = NullValueHandling.Ignore)]
        public List<ProductivityWeeklyTargetValues> assetProductivitySettings { get; set; }

        public Guid? UserUID { get; set; }
        public Guid? CustomerUID { get; set; }
    }
}
