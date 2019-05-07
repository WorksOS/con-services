using System;
using VSS.VisionLink.Interfaces.Events.OrgHierarchy.Context;

namespace VSS.VisionLink.Interfaces.Events.OrgHierarchy.Operations
{
  public class OrgRemovedByCustomerUidEvent
  {
    public TimestampDetail Timestamp { get; set; }
    public Guid CustomerUID { get; set; }
  }
}
