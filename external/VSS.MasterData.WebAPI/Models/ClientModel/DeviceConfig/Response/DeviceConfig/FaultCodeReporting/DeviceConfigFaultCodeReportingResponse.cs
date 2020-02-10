using CommonModel.Error;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.FaultCodeReporting
{
	public class DeviceConfigFaultCodeReportingResponse : BaseResponse<DeviceConfigFaultCodeReportingDetails, AssetErrorInfo>
    {
        //Default constructor added for the deserialization
        public DeviceConfigFaultCodeReportingResponse() { }

        public DeviceConfigFaultCodeReportingResponse(IList<AssetErrorInfo> errors) : base(errors) { }

        public DeviceConfigFaultCodeReportingResponse(AssetErrorInfo error) : base(error) { }

        public DeviceConfigFaultCodeReportingResponse(IEnumerable<DeviceConfigFaultCodeReportingDetails> movingThresholds, IList<AssetErrorInfo> errors = null) : base(movingThresholds, errors) { }

        [JsonProperty("deviceConfigFaultCodeReportingModes", NullValueHandling = NullValueHandling.Ignore)]
        public override IEnumerable<DeviceConfigFaultCodeReportingDetails> Lists { get; set; }
    }
}
