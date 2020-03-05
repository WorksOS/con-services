using ClientModel.Response;
using CommonModel.AssetSettings;
using CommonModel.Error;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.AssetSettings.Response.ProductivityTargetsResponse
{
	public class GetProductivityTargetsResponse : BaseResponse<AssetSettingsBase, AssetErrorInfo>
    {
        public GetProductivityTargetsResponse(IList<AssetErrorInfo> errors): base(errors) { }

        public GetProductivityTargetsResponse(AssetErrorInfo error) : base(error) { }

        public GetProductivityTargetsResponse(IList<AssetSettingsBase> lists) : base(lists) { }

        [JsonProperty("assetProductivitySettings", NullValueHandling = NullValueHandling.Ignore)]
        public override IList<AssetSettingsBase> Lists { get; set; }
        
    }
}
