using ClientModel.Response;
using CommonModel.Error;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.AssetSettings.Response.AssetTargets
{
	public class AssetEstimatedPayloadPerCycleSettingsResponse : BaseResponse<AssetSettingsResponse, AssetErrorInfo>
    {
        [JsonProperty("assetEstimatedPayloadPerCycleSettings", NullValueHandling = NullValueHandling.Ignore)]
        public override IList<AssetSettingsResponse> Lists { get; set; }

        public AssetEstimatedPayloadPerCycleSettingsResponse(IList<AssetSettingsResponse> lists, IList<AssetErrorInfo> errors) : base(lists) { this.Errors = errors; }

        public AssetEstimatedPayloadPerCycleSettingsResponse(IList<AssetErrorInfo> errors) : base(errors) { }

        public AssetEstimatedPayloadPerCycleSettingsResponse(AssetErrorInfo error) : base(error) { }

        public AssetEstimatedPayloadPerCycleSettingsResponse(IList<AssetSettingsResponse> lists) : base(lists) { }
    }
}
