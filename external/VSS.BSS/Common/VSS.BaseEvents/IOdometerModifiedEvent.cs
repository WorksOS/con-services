using System;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;

namespace VSS.BaseEvents
{
  public interface IOdometerModifiedEvent : IEndpointDestinedEvent
  {
    string DeviceId { get; set; }
    DeviceTypeEnum DeviceType { get; set; }
    DateTime TimestampUtc { get; set; }

    double? MilesBefore { get; set; }
    double MilesAfter { get; set; }
  }
}
