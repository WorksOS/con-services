using CommonModel.DeviceSettings;
using Newtonsoft.Json;
using System;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.ReportingSchedule
{
	public class DeviceConfigReportingScheduleDetails : DeviceConfigResponseBase
    {
        [JsonProperty("dailyReportingTime", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ValueWithPendingFlag<TimeSpan?> DailyReportingTime { get; set; }
        [JsonProperty("dailyLocationReportingFrequency", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ValueWithPendingFlag<int?> DailyLocationReportingFrequency { get; set; }
        [JsonProperty("hourMeterFuelReport", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ValueWithPendingFlag<string> HourMeterFuelReport { get; set; }
        [JsonProperty("reportAssetStartStop", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ValueWithPendingFlag<bool?> ReportAssetStartStop { get; set; }
        [JsonProperty("globalGram", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ValueWithPendingFlag<bool?> GlobalGram { get; set; }
    }
}

