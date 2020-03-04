using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.Meters
{
    public class DeviceConfigMetersDetails : DeviceConfigResponseBase
    {
        [JsonProperty("smhOdometerConfig", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SmhOdometerConfig { get; set; }
        [JsonProperty("odometer", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public MeterModel OdoMeter { get; set; }
        [JsonProperty("hoursMeter", NullValueHandling = NullValueHandling.Ignore)]
        public MeterModel HoursMeter { get; set; }
    }
}
