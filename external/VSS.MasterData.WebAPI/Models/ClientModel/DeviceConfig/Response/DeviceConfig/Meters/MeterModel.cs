using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.Meters
{
    public class MeterModel
    {
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public double? Value { get; set; }
        [JsonProperty("canUpdate", NullValueHandling = NullValueHandling.Ignore)]
        public bool CanUpdate { get; set; } //TODO: add setter after adding meters workflow
        [JsonProperty("allowBackward", NullValueHandling = NullValueHandling.Ignore)]
        public bool AllowBackward { get; set; } //TODO: add setter after adding meters workflow
    }
}
