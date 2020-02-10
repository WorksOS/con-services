using CommonModel.Error;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.MovingThreshold
{
    public class DeviceConfigMovingThresholdResponse : BaseResponse<DeviceConfigMovingThresholdDetails, AssetErrorInfo>
    {
        //Default constructor added for the deserialization
        public DeviceConfigMovingThresholdResponse() { }

        public DeviceConfigMovingThresholdResponse(IList<AssetErrorInfo> errors) : base(errors) { }

        public DeviceConfigMovingThresholdResponse(AssetErrorInfo error) : base(error) { }

        public DeviceConfigMovingThresholdResponse(IEnumerable<DeviceConfigMovingThresholdDetails> movingThresholds, IList<AssetErrorInfo> errors = null) : base(movingThresholds, errors) { }

        [JsonProperty("deviceConfigMovingThresholds", NullValueHandling = NullValueHandling.Ignore)]
        public override IEnumerable<DeviceConfigMovingThresholdDetails> Lists { get; set; }
    }
}
