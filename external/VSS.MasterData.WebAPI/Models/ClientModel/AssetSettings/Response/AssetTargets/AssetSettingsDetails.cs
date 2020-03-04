using Newtonsoft.Json;
using System;

namespace ClientModel.AssetSettings.Response.AssetTargets
{
	public class AssetSettingsDetails
    {
        [JsonProperty("assetUid", NullValueHandling = NullValueHandling.Include)]
        public string AssetUID { get; set; }
        [JsonProperty("assetId", NullValueHandling = NullValueHandling.Include)]
        public string AssetName { get; set; }
        [JsonProperty("assetSerialNumber", NullValueHandling = NullValueHandling.Include)]
        public string SerialNumber { get; set; }
        [JsonProperty("assetModel", NullValueHandling = NullValueHandling.Include)]
        public string Model { get; set; }
        [JsonProperty("assetMakeCode", NullValueHandling = NullValueHandling.Include)]
        public string MakeCode { get; set; }
        [JsonProperty("assetIconKey", NullValueHandling = NullValueHandling.Include)]
        public int? IconKey { get; set; }
        [JsonProperty("deviceSerialNumber", NullValueHandling = NullValueHandling.Include)]
        public string DeviceSerialNumber { get; set; }
		[JsonProperty("devicetype", NullValueHandling = NullValueHandling.Include)]
		public string DeviceType { get; set; }
		[JsonProperty("targetStatus", NullValueHandling = NullValueHandling.Include)]
        public bool TargetStatus { get; set; }
        [JsonProperty("dailyLocationReportingFrequency", NullValueHandling = NullValueHandling.Ignore)]
        public long? DailyLocationReportingFrequency { get; set; }
        [JsonProperty("dailyReportingTime", NullValueHandling = NullValueHandling.Ignore)]
        public string DailyReportingTime { get; set; }
        [JsonProperty("diagnosticReportFrequency", NullValueHandling = NullValueHandling.Ignore)]
        public string DiagnosticReportFrequency { get; set; }
        [JsonProperty("eventDiagnosticFilterInterval", NullValueHandling = NullValueHandling.Ignore)]
        public long? EventDiagnosticFilterInterval { get; set; }
        [JsonProperty("globalGram", NullValueHandling = NullValueHandling.Ignore)]
        public bool? GlobalGram { get; set; }
        [JsonProperty("highSeverityEvents", NullValueHandling = NullValueHandling.Ignore)]
        public string HighSeverityEvents { get; set; }
        [JsonProperty("hourMeterFuelReport", NullValueHandling = NullValueHandling.Ignore)]
        public string HourMeterFuelReport { get; set; }
        [JsonProperty("hoursMeter", NullValueHandling = NullValueHandling.Ignore)]
        public Decimal? HoursMeter { get; set; }
        [JsonProperty("lowSeverityEvents", NullValueHandling = NullValueHandling.Ignore)]
        public string LowSeverityEvents { get; set; }
        [JsonProperty("maintenanceModeDuration", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaintenanceModeDuration { get; set; }
        [JsonProperty("mediumSeverityEvents", NullValueHandling = NullValueHandling.Ignore)]
        public string MediumSeverityEvents { get; set; }
        [JsonProperty("movingOrStoppedThreshold", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Decimal? MovingOrStoppedThreshold { get; set; }
        [JsonProperty("movingThresholdsDuration", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? MovingThresholdsDuration { get; set; }
        [JsonProperty("movingThresholdsRadius", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Decimal? MovingThresholdsRadius { get; set; }
        [JsonProperty("nextSentEventInHours", NullValueHandling = NullValueHandling.Ignore)]
        public int? NextSentEventInHours { get; set; }
        [JsonProperty("odometer", NullValueHandling = NullValueHandling.Ignore)]
        public Decimal? Odometer { get; set; }
        [JsonProperty("reportAssetStartStop", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ReportAssetStartStop { get; set; }
        [JsonProperty("sentUTC", NullValueHandling = NullValueHandling.Ignore)]
        public string SentUTC { get; set; }
        [JsonProperty("smhOdometerConfig", NullValueHandling = NullValueHandling.Ignore)]
        public string SMHOdometerConfig { get; set; }
        [JsonProperty("speedThreshold", NullValueHandling = NullValueHandling.Ignore)]
        public int? SpeedThreshold { get; set; }
        [JsonProperty("speedThresholdDuration", NullValueHandling = NullValueHandling.Ignore)]
        public int? SpeedThresholdDuration { get; set; }
        [JsonProperty("speedThresholdEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SpeedThresholdEnabled { get; set; }
        [JsonProperty("startTime", NullValueHandling = NullValueHandling.Ignore)]
        public string StartTime { get; set; }
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Status { get; set; }
        [JsonProperty("workDefinition", NullValueHandling = NullValueHandling.Ignore)]
        public int? WorkDefinition { get; set; }
        [JsonProperty("configuredSwitches", NullValueHandling = NullValueHandling.Ignore)]
        public int ConfiguredSwitches { get; set; }
        [JsonProperty("totalSwitches", NullValueHandling = NullValueHandling.Ignore)]
        public int TotalSwitches { get; set; }
        [JsonProperty("securityStatus", NullValueHandling = NullValueHandling.Ignore)]
        public int? SecurityStatus { get; set; }
        [JsonProperty("securityMode", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SecurityMode { get; set; }
        [JsonProperty("pendingDeviceConfigInfo", NullValueHandling = NullValueHandling.Ignore)]
        public PendingDeviceConfigDetails PendingDeviceConfigInfo { get; set; }        
    }
}