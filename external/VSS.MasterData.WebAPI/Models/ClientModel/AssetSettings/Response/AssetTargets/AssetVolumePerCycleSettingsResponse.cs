using ClientModel.Response;
using CommonModel.Error;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.AssetSettings.Response.AssetTargets
{
	public class AssetVolumePerCycleSettingsResponse : BaseResponse<AssetSettingsResponse, AssetErrorInfo>
    {
        public AssetVolumePerCycleSettingsResponse(IList<AssetSettingsResponse> lists, IList<AssetErrorInfo> errors) : base(lists) { this.Errors = errors; }

        public AssetVolumePerCycleSettingsResponse(IList<AssetErrorInfo> errors) : base(errors) { }

        public AssetVolumePerCycleSettingsResponse(AssetErrorInfo error) : base(error) { }

        public AssetVolumePerCycleSettingsResponse(IList<AssetSettingsResponse> lists) : base(lists) { }

        [JsonProperty("assetVolumePerCycleSettings", NullValueHandling = NullValueHandling.Ignore)]
        public override IList<AssetSettingsResponse> Lists { get; set; }
    }
}
