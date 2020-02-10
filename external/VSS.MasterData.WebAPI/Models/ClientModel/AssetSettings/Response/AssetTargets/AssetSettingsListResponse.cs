using CommonModel.Error;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.AssetSettings.Response.AssetTargets
{
	public class AssetSettingsListResponse : SearchBaseResponse<AssetSettingsDetails, ErrorInfo>
    {
        public AssetSettingsListResponse() { }
        public AssetSettingsListResponse(ErrorInfo error) : base(error) { }

        public AssetSettingsListResponse(List<ErrorInfo> errors) : base(errors) { }

        public AssetSettingsListResponse(List<AssetSettingsDetails> lists) : base(lists) { }

        public AssetSettingsListResponse(List<AssetSettingsDetails> lists, int currentPageSize, int currentPageNumber, long totalRows) : base(lists, currentPageSize, currentPageNumber, totalRows) { }

        [JsonProperty("assetSettings", NullValueHandling = NullValueHandling.Ignore)]
        public override IList<AssetSettingsDetails> Lists { get; set; }
    }
}
