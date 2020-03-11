using Infrastructure.Common.DeviceSettings.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModel.DeviceConfig.Request.DeviceConfig.FaultCodeReporting
{
    public class DeviceConfigFaultCodeReportingRequest : DeviceConfigRequestBase
    {
        [JsonProperty("lowSeverityEvents", NullValueHandling = NullValueHandling.Ignore)]
        public FaultCodeReportingEventSeverity? LowSeverityEvents { get; set; }
        [JsonProperty("mediumSeverityEvents", NullValueHandling = NullValueHandling.Ignore)]
        public FaultCodeReportingEventSeverity? MediumSeverityEvents { get; set; }
        [JsonProperty("highSeverityEvents", NullValueHandling = NullValueHandling.Ignore)]
        public FaultCodeReportingEventSeverity? HighSeverityEvents { get; set; }
        [JsonProperty("diagnosticReportFrequency", NullValueHandling = NullValueHandling.Ignore)]
        public FaultCodeReportingEventSeverity? DiagnosticReportFrequency { get; set; }
        [JsonProperty("nextSentEventInHours", NullValueHandling = NullValueHandling.Ignore)]
        public Nullable<int> NextSentEventInHours { get; set; }
        [JsonProperty("eventDiagnosticFilterInterval", NullValueHandling = NullValueHandling.Ignore)]
        public Nullable<int> EventDiagnosticFilterInterval { get; set; }
    }
}
