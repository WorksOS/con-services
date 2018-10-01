using System;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.GridFabric.Affinity
{
  public class SegmentRetirementQueueKey : ISegmentRetirementQueueKey
  {
    public Guid ProjectID { get; set; }

    public long InsetUTCasLong { get; set; }

    public override string ToString() => $"Project: {ProjectID}, InsertUTCasLong:{InsetUTCasLong}";
  }
}
