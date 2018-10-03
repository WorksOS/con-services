using System;
using Apache.Ignite.Core.Cache.Configuration;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.GridFabric.Affinity
{
  public class SegmentRetirementQueueKey : ISegmentRetirementQueueKey
  {
    public Guid ProjectUID { get; set; }

    [QuerySqlField(IsIndexed = true)]
    public long InsertUTCAsLong { get; set; }

    public override string ToString() => $"Project: {ProjectUID}, InsertUTCAsLong:{InsertUTCAsLong}";
  }
}
