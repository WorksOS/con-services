using System;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;

namespace VSS.BaseEvents
{
  public interface ISetDailyReportFrequencyEvent : IEndpointDestinedEvent
  {
    string DeviceId { get; set; }
    DeviceTypeEnum DeviceType { get; set; }
    DateTime TimestampUtc { get; set; }
    int Value { get; set; }
  }
}