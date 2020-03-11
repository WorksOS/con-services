using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.Parameter
{
    public class Attributes
    {
        [JsonProperty("attributeId", NullValueHandling = NullValueHandling.Ignore)]
        public ulong AttributeId { get; set; }
        [JsonProperty("attributeName", NullValueHandling = NullValueHandling.Ignore)]
        public string AttributeName { get; set; }
        [JsonProperty("displayName", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }
    }
}
