using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.ParameterGroup
{
    public class ParameterGroupDetails : DeviceConfigResponseBase
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong Id { get; set; }
        [JsonProperty("multipleDevicesAllowed", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsMultiDeviceTypeSupport { get; set; }
        [JsonProperty("deviceBasedGroup", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsDeviceParamGroup { get; set; }
    }
}
