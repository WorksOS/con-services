using System.IO;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Reports.Gridded;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Responses
{
  public class OffsetStatistics_ApplicationService : GriddedReportDataRow
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
