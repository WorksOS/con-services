using System;
using VSS.VisionLink.Interfaces.Events.OrgHierarchy.Context;

namespace VSS.VisionLink.Interfaces.Events.OrgHierarchy.Operations
{
  public class OrgDissociatedEvent
  {
    public TimestampDetail Timestamp { get; set; }
    public Guid CustomerUID { get; set; }
    public Guid ParentOrgUid { get; set; }
    public Guid DissociatedNodeOrgUid { get; set; }
  }
}
