using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModel.DeviceConfig.Request.DeviceConfig.MaintenanceMode
{
    public class DeviceConfigMaintenanceModeRequest : DeviceConfigRequestBase
    {
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Status { get; set; }
        [JsonProperty("startTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime StartTime { get; set; }
        [JsonProperty("maintenanceModeDuration", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaintenanceModeDuration { get; set; }
    }
}
