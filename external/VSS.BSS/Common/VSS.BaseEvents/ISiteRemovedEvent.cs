using System;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;

namespace VSS.BaseEvents
{
  public interface ISiteRemovedEvent : IEndpointDestinedEvent
  {
    Guid SiteId { get; set; }

    DeviceTypeEnum DeviceType { get; set; }

    string DeviceId { get; set; }

    DateTime TimestampUtc { get; set; }
  }
}
