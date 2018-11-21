using System;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
  public class SegmentRetirementQueueQueryFilter : ICacheEntryFilter<ISegmentRetirementQueueKey, SegmentRetirementQueueItem>, IBinarizable, IFromToBinary, IEquatable<SegmentRetirementQueueQueryFilter>
  {
    private const byte kVersionNumber = 1;

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

    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());
    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte(kVersionNumber);
      writer.WriteLong(retirementDateAsLong);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      var version = reader.ReadByte();

      if (version != kVersionNumber)
        throw new TRexSerializationVersionException(kVersionNumber, version);

      retirementDateAsLong = reader.ReadLong();
    }

    public bool Equals(SegmentRetirementQueueQueryFilter other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return retirementDateAsLong == other.retirementDateAsLong;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((SegmentRetirementQueueQueryFilter) obj);
    }

    public override int GetHashCode()
    {
      return retirementDateAsLong.GetHashCode();
    }
  }
}
