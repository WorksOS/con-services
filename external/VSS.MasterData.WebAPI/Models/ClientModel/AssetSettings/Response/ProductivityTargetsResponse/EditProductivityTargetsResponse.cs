using ClientModel.Response;
using CommonModel.Error;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.AssetSettings.Response.ProductivityTargetsResponse
{
	public class EditProductivityTargetsResponse : BaseResponse<string, ErrorInfo>
    {
        public EditProductivityTargetsResponse(IList<ErrorInfo> errors): base(errors) { }

        public EditProductivityTargetsResponse(ErrorInfo error) : base(error) { }

        public EditProductivityTargetsResponse(IList<string> lists) : base(lists) { }

        [JsonProperty("assetUIDs", NullValueHandling = NullValueHandling.Ignore)]
        public override IList<string> Lists { get; set; }
    }
}
