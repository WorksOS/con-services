using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModel.AssetSettings.Response.AssetTargets
{
    public class PendingDeviceConfigDetails
    {
        [JsonProperty("faultCodeReporting", NullValueHandling = NullValueHandling.Ignore)]
        public GroupDetail FaultCodeReporting { get; set; }
        [JsonProperty("reportingSchedule", NullValueHandling = NullValueHandling.Ignore)]
        public GroupDetail ReportingSchedule { get; set; }
        [JsonProperty("meters", NullValueHandling = NullValueHandling.Ignore)]
        public GroupDetail Meters { get; set; }
        [JsonProperty("switches", NullValueHandling = NullValueHandling.Ignore)]
        public GroupDetail Switches { get; set; }
        [JsonProperty("maintenanceMode", NullValueHandling = NullValueHandling.Ignore)]
        public GroupDetail MaintenanceMode { get; set; }
        [JsonProperty("movingThresholds", NullValueHandling = NullValueHandling.Ignore)]
        public GroupDetail MovingThresholds { get; set; }
        [JsonProperty("speedingThresholds", NullValueHandling = NullValueHandling.Ignore)]
        public GroupDetail SpeedingThresholds { get; set; }
        [JsonProperty("assetSecurity", NullValueHandling = NullValueHandling.Ignore)]
        public GroupDetail AssetSecurity { get; set; }

    }
    public class GroupDetail
    {
        [JsonProperty("lastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? LastUpdatedOn { get; set; }
    }
}
