using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Models.Reports;

namespace VSS.TRex.Reports.Gridded
{
  public class GriddedReportDataRow : GriddedReportDataRowBase
  {
    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteDouble(Northing);
      writer.WriteDouble(Easting);
      writer.WriteDouble(Elevation);
      writer.WriteDouble(CutFill);
      writer.WriteShort(Cmv);
      writer.WriteShort(Mdp);
      writer.WriteShort(PassCount);
      writer.WriteShort(Temperature);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      Northing = reader.ReadDouble();
      Easting = reader.ReadDouble();
      Elevation = reader.ReadDouble();
      CutFill = reader.ReadDouble();
      Cmv = reader.ReadShort();
      Mdp = reader.ReadShort();
      PassCount = reader.ReadShort();
      Temperature = reader.ReadShort();
    }
  }
}
