using Newtonsoft.Json;

namespace ClientModel.DeviceConfig.Request.DeviceConfig.Parameter
{
    public class DeviceConfigParameterRequest : DeviceConfigRequestBase
    {
        [JsonProperty("parameterGroupID", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ParameterGroupID { get; set; }
    }
}
