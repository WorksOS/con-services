using CommonModel.Error;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.Asset_Security
{
	public class DeviceConfigAssetSecurityResponse : BaseResponse<DeviceConfigAssetSecurityDetails, AssetErrorInfo>
    {
        //Default constructor added for the deserialization
        public DeviceConfigAssetSecurityResponse() { }

        public DeviceConfigAssetSecurityResponse(IList<AssetErrorInfo> errors) : base(errors) { }

        public DeviceConfigAssetSecurityResponse(AssetErrorInfo error) : base(error) { }

        public DeviceConfigAssetSecurityResponse(IEnumerable<DeviceConfigAssetSecurityDetails> assetSecurityDetails, IList<AssetErrorInfo> errors = null) : base(assetSecurityDetails, errors) { }

        [JsonProperty("deviceConfigAssetSecurityDetails", NullValueHandling = NullValueHandling.Ignore)]
        public override IEnumerable<DeviceConfigAssetSecurityDetails> Lists { get; set; }
    }
}
