using System;
using VSS.VisionLink.Interfaces.Events.OrgHierarchy.Context;

namespace VSS.VisionLink.Interfaces.Events.OrgHierarchy.Operations
{
  public class OrgAssociatedEvent
  {
    public TimestampDetail Timestamp { get; set; }
    public Guid CustomerUID { get; set; }
    public Guid ParentOrgUid { get; set; }
    public Guid AssociatedNodeOrgUid { get; set; }
  }
}
