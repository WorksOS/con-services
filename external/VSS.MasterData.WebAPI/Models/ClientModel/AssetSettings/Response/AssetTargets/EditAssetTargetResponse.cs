using ClientModel.Response;
using CommonModel.Error;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.AssetSettings.Response.AssetTargets
{
	public class EditAssetTargetResponse : BaseResponse<string, AssetErrorInfo>
    {
        public EditAssetTargetResponse(IList<AssetErrorInfo> errors): base(errors) { }

        public EditAssetTargetResponse(AssetErrorInfo error) : base(error) { }

        public EditAssetTargetResponse(IList<string> lists) : base(lists) { }

        [JsonProperty("assetUIDs", NullValueHandling = NullValueHandling.Ignore)]
        public override IList<string> Lists { get; set; }
    }
}
