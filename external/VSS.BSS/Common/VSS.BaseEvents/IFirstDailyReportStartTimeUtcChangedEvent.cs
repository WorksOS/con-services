using System;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;

namespace VSS.BaseEvents
{
  public interface IFirstDailyReportStartTimeUtcChangedEvent : IEndpointDestinedEvent
  {
    string DeviceId { get; set; }
    DeviceTypeEnum DeviceType { get; set; }
    DateTime TimestampUtc { get; set; }

    DateTime DailyReportTimeUTC { get; set; }
  }
}
