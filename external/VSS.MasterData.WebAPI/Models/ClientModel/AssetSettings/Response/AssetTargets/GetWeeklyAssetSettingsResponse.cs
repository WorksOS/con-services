using CommonModel.AssetSettings;
using CommonModel.Error;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.AssetSettings.Response.AssetTargets
{
	public class GetWeeklyAssetSettingsResponse
    {
        public List<AssetSettingsWeeklyTargets> assetTargetSettings;
        [JsonProperty("errors")]
        public List<AssetErrorInfo> Errors;
    }
}
