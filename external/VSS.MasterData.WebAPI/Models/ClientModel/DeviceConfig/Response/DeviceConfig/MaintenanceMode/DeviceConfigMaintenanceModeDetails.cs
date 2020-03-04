using Newtonsoft.Json;
using System;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.MaintenanceMode
{
    public class DeviceConfigMaintenanceModeDetails : DeviceConfigResponseBase
    {
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public bool Status { get; set; }
        //[JsonProperty("startTime", NullValueHandling = NullValueHandling.Ignore)]
        //public TimeSpan StartTime { get; set; }
        [JsonProperty("maintenanceModeDuration", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaintenanceModeDuration { get; set; }
    }
}