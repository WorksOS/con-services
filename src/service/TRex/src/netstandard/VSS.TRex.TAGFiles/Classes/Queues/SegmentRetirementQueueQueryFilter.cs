using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
  public class SegmentRetirementQueueQueryFilter : VersionCheckedBinarizableSerializationBase, ICacheEntryFilter<ISegmentRetirementQueueKey, SegmentRetirementQueueItem>
  {
    private const byte VERSION_NUMBER = 1;

    public long retirementDateAsLong;

    public SegmentRetirementQueueQueryFilter()
    {
    }

    public SegmentRetirementQueueQueryFilter(long retirementDate)
    {
      retirementDateAsLong = retirementDate;
    }

    public bool Invoke(ICacheEntry<ISegmentRetirementQueueKey, SegmentRetirementQueueItem> entry)
    {
      return entry.Key.InsertUTCAsLong < retirementDateAsLong;
    }

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteLong(retirementDateAsLong);
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        retirementDateAsLong = reader.ReadLong();
      }
    }
  }
}
