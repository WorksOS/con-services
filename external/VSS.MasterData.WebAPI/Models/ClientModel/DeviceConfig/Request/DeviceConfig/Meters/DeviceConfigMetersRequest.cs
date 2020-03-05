using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Common.DeviceSettings.Enums;

namespace ClientModel.DeviceConfig.Request.DeviceConfig.Meters
{
    public class DeviceConfigMetersRequest : DeviceConfigRequestBase, IServiceRequest
    {
        [JsonProperty("smhOdometerConfig", NullValueHandling = NullValueHandling.Ignore)]
        public MetersSmhOdometerConfig? SmhOdometerConfig { get; set; }
        [JsonProperty("hoursMeter", NullValueHandling = NullValueHandling.Ignore)]
        public MeterDetails HoursMeter { get; set; }
        [JsonProperty("odometer", NullValueHandling = NullValueHandling.Ignore)]
        public MeterDetails OdoMeter { get; set; }
    }
}

