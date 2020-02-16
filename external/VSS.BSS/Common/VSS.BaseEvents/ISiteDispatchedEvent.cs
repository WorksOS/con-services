using System;
using System.Collections.Generic;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;

namespace VSS.BaseEvents
{
  public interface ISiteDispatchedEvent : IEndpointDestinedEvent
  {
    Guid SiteId { get; set; }

    string Name { get; set; }

    IList<Point> Polygon { get; set; }

    DeviceTypeEnum DeviceType { get; set; }

    string DeviceId { get; set; }

    DateTime TimestampUtc { get; set; }
  }
}
