using System;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;

namespace VSS.BaseEvents
{
  public interface IDisableMaintenanceModeEvent : IEndpointDestinedEvent
  {
    string DeviceId { get; set; }
    DeviceTypeEnum DeviceType { get; set; }
    DateTime TimestampUtc { get; set; }
  }
}
