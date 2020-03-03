using CommonModel.Error;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.Parameter
{
	public class DeviceConfigParameterResponse : BaseResponse<ParameterDetails, ErrorInfo>
    {
        //Default constructor added for the deserialization
        public DeviceConfigParameterResponse() { }

        public DeviceConfigParameterResponse(IList<ErrorInfo> errors) : base(errors) { }

        public DeviceConfigParameterResponse(ErrorInfo error) : base(error) { }

        public DeviceConfigParameterResponse(IEnumerable<ParameterDetails> parameters, IList<ErrorInfo> errors = null) : base(parameters, errors) { }

        [JsonProperty("deviceConfigParameters", NullValueHandling = NullValueHandling.Ignore)]
        public override IEnumerable<ParameterDetails> Lists { get; set; }
    }
}