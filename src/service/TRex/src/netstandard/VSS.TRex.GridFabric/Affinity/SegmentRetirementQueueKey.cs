using System;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Configuration;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Exceptions;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.GridFabric.Affinity
{
  public class SegmentRetirementQueueKey : ISegmentRetirementQueueKey, IBinarizable, IFromToBinary, IEquatable<SegmentRetirementQueueKey>
  {
    private const byte VERSION_NUMBER = 1;

    public Guid ProjectUID { get; set; }

    [QuerySqlField(IsIndexed = true)]
    public long InsertUTCAsLong { get; set; }

    public override string ToString() => $"Project: {ProjectUID}, InsertUTCAsLong:{InsertUTCAsLong}";

    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte(VERSION_NUMBER);
      writer.WriteGuid(ProjectUID);
      writer.WriteLong(InsertUTCAsLong);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      int version = reader.ReadByte();

      if (version != VERSION_NUMBER)
        throw new TRexSerializationVersionException(VERSION_NUMBER, version);

      ProjectUID = reader.ReadGuid() ?? Guid.Empty;
      InsertUTCAsLong = reader.ReadLong();
    }

    public bool Equals(SegmentRetirementQueueKey other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return ProjectUID.Equals(other.ProjectUID) && InsertUTCAsLong == other.InsertUTCAsLong;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((SegmentRetirementQueueKey) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (ProjectUID.GetHashCode() * 397) ^ InsertUTCAsLong.GetHashCode();
      }
    }
  }
}
