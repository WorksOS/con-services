using System;
using System.IO;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Reports.Gridded;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Responses
{
  public class OffsetStatistics_ApplicationService : GriddedReportDataRow, IEquatable<OffsetStatistics_ApplicationService>
  {
    public new void Write(BinaryWriter writer)
    {
      base.Write(writer);
    }

    public new void Read(BinaryReader reader)
    {
      base.Read(reader);
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public new void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public new void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);
    }

    public bool Equals(OffsetStatistics_ApplicationService other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return true;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((OffsetStatistics_ApplicationService)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        return hashCode;
      }
    }
  }
}
