using CommonModel.Error;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.ReportingSchedule
{
    public class DeviceConfigReportingScheduleResponse : BaseResponse<DeviceConfigReportingScheduleDetails, AssetErrorInfo>
    {
        //Default constructor added for the deserialization
        public DeviceConfigReportingScheduleResponse() { }

        public DeviceConfigReportingScheduleResponse(IList<AssetErrorInfo> errors) : base(errors) { }

        public DeviceConfigReportingScheduleResponse(AssetErrorInfo error) : base(error) { }

        public DeviceConfigReportingScheduleResponse(IEnumerable<DeviceConfigReportingScheduleDetails> parameterGroups, IList<AssetErrorInfo> errors = null) : base(parameterGroups, errors) { }

        [JsonProperty("deviceConfigReportingSchedules", NullValueHandling = NullValueHandling.Ignore)]
        public override IEnumerable<DeviceConfigReportingScheduleDetails> Lists { get; set; }
    }
}
