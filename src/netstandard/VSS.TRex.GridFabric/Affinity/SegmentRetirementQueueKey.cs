using System;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.GridFabric.Affinity
{
  public class SegmentRetirementQueueKey : ISegmentRetirementQueueKey
  {
    public Guid ProjectID { get; set; }
  }
}
