using System.IO;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Reports.Gridded;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Responses
{
  /// <summary>
  /// Contains the prepared result for the client to consume
  /// </summary>
  public class StationOffsetReportData_ApplicationService : ReportTypeData
  {
    public int NumberOfRows { get; set; }
    public StationOffsetReportDataRows_ApplicationService Rows { get; private set; }

    public StationOffsetReportData_ApplicationService()
    {
      base.Clear();
      Clear();
    }

    public new void Clear()
    {
      Rows = new StationOffsetReportDataRows_ApplicationService();
      NumberOfRows = Rows.Count;
    }

    public new void Write(BinaryWriter writer)
    {
      NumberOfRows = Rows.Count;
      writer.Write(NumberOfRows);
      Rows.Write(writer);
      base.Write(writer);
    }

    public new void Read(BinaryReader reader)
    {
      NumberOfRows = reader.ReadInt32();
      Rows = new StationOffsetReportDataRows_ApplicationService();
      Rows.Read(reader, NumberOfRows);
      base.Read(reader);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ NumberOfRows.GetHashCode();
        hashCode = (hashCode * 397) ^ Rows.GetHashCode();
        return hashCode;
      }
    }

  }
}

