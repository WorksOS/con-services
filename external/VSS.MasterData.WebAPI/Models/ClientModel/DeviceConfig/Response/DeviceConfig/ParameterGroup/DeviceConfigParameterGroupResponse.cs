using CommonModel.Error;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.ParameterGroup
{
    public class DeviceConfigParameterGroupResponse : BaseResponse<ParameterGroupDetails, ErrorInfo>
    {
        //Default constructor added for the deserialization
        public DeviceConfigParameterGroupResponse() { }

        public DeviceConfigParameterGroupResponse(IList<ErrorInfo> errors) : base(errors) { }

        public DeviceConfigParameterGroupResponse(ErrorInfo error) : base(error) { }

        public DeviceConfigParameterGroupResponse(IEnumerable<ParameterGroupDetails> parameterGroups, IList<ErrorInfo> errors = null) : base(parameterGroups, errors) { }

        [JsonProperty("deviceConfigParameterGroups", NullValueHandling = NullValueHandling.Ignore)]
        public override IEnumerable<ParameterGroupDetails> Lists { get; set; }
    }
}