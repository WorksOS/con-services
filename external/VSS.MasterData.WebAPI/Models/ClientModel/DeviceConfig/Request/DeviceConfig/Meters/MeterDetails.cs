using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModel.DeviceConfig.Request.DeviceConfig.Meters
{
    public class MeterDetails
    {
        [JsonProperty("proposedValue", NullValueHandling = NullValueHandling.Ignore)]
        public double? ProposedValue { get; set; } //After
        [JsonProperty("currentValue", NullValueHandling = NullValueHandling.Ignore)]
        public double? CurrentValue { get; set; } //Before
    }
}
