using Newtonsoft.Json;
using System;
using Infrastructure.Common.DeviceSettings.Enums;

namespace ClientModel.DeviceConfig.Request.DeviceConfig.ReportingSchedule
{
    public class DeviceConfigReportingScheduleRequest : DeviceConfigRequestBase
    {
        [JsonProperty("dailyReportingTime", NullValueHandling = NullValueHandling.Ignore)]
        public TimeSpan? DailyReportingTime { get; set; }
        [JsonProperty("dailyLocationReportingFrequency", NullValueHandling = NullValueHandling.Ignore)]
        public ReportingScheduleDailyLocationReportingFrequency? DailyLocationReportingFrequency { get; set; }
        [JsonProperty("hourMeterFuelReport", NullValueHandling = NullValueHandling.Ignore)]
        public ReportingScheduleHourMeterFuelReport? HourMeterFuelReport { get; set; }
        [JsonProperty("reportAssetStartStop", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ReportAssetStartStop { get; set; }
        [JsonProperty("globalGram", NullValueHandling = NullValueHandling.Ignore)]
        public bool? GlobalGram { get; set; }     
    }
}
