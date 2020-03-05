using ClientModel.Response;
using CommonModel.Error;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.AssetSettings.Response.AssetTargets
{
	public class AssetMileageSettingsResponse : BaseResponse<AssetSettingsResponse, AssetErrorInfo>
    {
        public AssetMileageSettingsResponse(IList<AssetSettingsResponse> lists, IList<AssetErrorInfo> errors) : base(lists) { this.Errors = errors; }

        public AssetMileageSettingsResponse(IList<AssetErrorInfo> errors): base(errors) { }

        public AssetMileageSettingsResponse(AssetErrorInfo error) : base(error) { }

        public AssetMileageSettingsResponse(IList<AssetSettingsResponse> lists) : base(lists) { }

        [JsonProperty("assetMileageSettings", NullValueHandling = NullValueHandling.Ignore)]
        public override IList<AssetSettingsResponse> Lists { get; set; }
    }
}
