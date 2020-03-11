using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.Parameter
{
    public class ParameterDetails : DeviceConfigResponseBase
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong Id { get; set; }
        [JsonProperty("groupName", NullValueHandling = NullValueHandling.Ignore)]
        public string ParameterGroupName { get; set; }
        [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
        public Attributes[] Attributes { get; set; }
        [JsonProperty("settings", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> settings { get; set; }
    }
}
