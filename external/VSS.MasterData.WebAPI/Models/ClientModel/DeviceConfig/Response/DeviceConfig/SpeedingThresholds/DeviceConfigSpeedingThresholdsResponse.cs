using CommonModel.Error;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.SpeedingThresholds
{
    public class DeviceConfigSpeedingThresholdsResponse : BaseResponse<DeviceConfigSpeedingThresholdsDetails, AssetErrorInfo>
    {
        //Default constructor added for the deserialization
        public DeviceConfigSpeedingThresholdsResponse() { }

        public DeviceConfigSpeedingThresholdsResponse(IList<AssetErrorInfo> errors) : base(errors) { }

        public DeviceConfigSpeedingThresholdsResponse(AssetErrorInfo error) : base(error) { }

        public DeviceConfigSpeedingThresholdsResponse(IEnumerable<DeviceConfigSpeedingThresholdsDetails> speedingThresholds, IList<AssetErrorInfo> errors = null) : base(speedingThresholds, errors) { }

        [JsonProperty("deviceConfigSpeedingThresholds", NullValueHandling = NullValueHandling.Ignore)]
        public override IEnumerable<DeviceConfigSpeedingThresholdsDetails> Lists { get; set; }
    }
}
