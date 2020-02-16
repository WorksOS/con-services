using System;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;

namespace VSS.BaseEvents
{
  public interface ILocationStatusUpdateRequestedEvent : IEndpointDestinedEvent
  {
    string DeviceId { get; set; }

    DeviceTypeEnum DeviceType { get; set; }

    DateTime TimestampUtc { get; set; }
  }
}
