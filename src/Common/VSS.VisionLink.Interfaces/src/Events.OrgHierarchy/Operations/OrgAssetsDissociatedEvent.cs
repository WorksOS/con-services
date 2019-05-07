using System;
using VSS.VisionLink.Interfaces.Events.OrgHierarchy.Context;

namespace VSS.VisionLink.Interfaces.Events.OrgHierarchy.Operations
{
  public class OrgAssetsDissociatedEvent
  {
    public TimestampDetail Timestamp { get; set; }
    public Guid CustomerUID { get; set; }
    public Guid OrgUid { get; set; }

    public Guid[] DissociatedAssetUids { get; set; }
  }
}
