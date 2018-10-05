using Apache.Ignite.Core.Cache;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
  public class SegmentRetirementQueueQueryFilter : ICacheEntryFilter<ISegmentRetirementQueueKey, SegmentRetirementQueueItem>
  {
    public long retirementDateAsLong;

    public SegmentRetirementQueueQueryFilter(long retirementDate) : base()
    {
      retirementDateAsLong = retirementDate;
    }

    public bool Invoke(ICacheEntry<ISegmentRetirementQueueKey, SegmentRetirementQueueItem> entry)
    {
      return entry.Key.InsertUTCAsLong < retirementDateAsLong;
    }
  }
}
