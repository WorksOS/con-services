using CommonModel.DeviceSettings;
using Newtonsoft.Json;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.FaultCodeReporting
{
	public class DeviceConfigFaultCodeReportingDetails : DeviceConfigResponseBase
    {
        [JsonProperty("lowSeverityEvents", NullValueHandling = NullValueHandling.Ignore)]
        public ValueWithPendingFlag<string> LowSeverityEvents { get; set; }
        [JsonProperty("mediumSeverityEvents", NullValueHandling = NullValueHandling.Ignore)]
        public ValueWithPendingFlag<string> MediumSeverityEvents { get; set; }
        [JsonProperty("highSeverityEvents", NullValueHandling = NullValueHandling.Ignore)]
        public ValueWithPendingFlag<string> HighSeverityEvents { get; set; }
        [JsonProperty("diagnosticReportFrequency", NullValueHandling = NullValueHandling.Ignore)]
        public ValueWithPendingFlag<string> DiagnosticReportFrequency { get; set; }
        [JsonProperty("nextSentEventInHours", NullValueHandling = NullValueHandling.Ignore)]
        public ValueWithPendingFlag<int?> NextSentEventInHours { get; set; }
        [JsonProperty("eventDiagnosticFilterInterval", NullValueHandling = NullValueHandling.Ignore)]
        public ValueWithPendingFlag<int?> EventDiagnosticFilterInterval { get; set; }
    }
}
