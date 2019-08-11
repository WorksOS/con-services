using System.IO;
using VSS.Productivity3D.Models.Models.Reports;

namespace VSS.Productivity3D.WebApi.Models.Report.ResultHandling
{
  public class StationOffsetReportData : ReportTypeData
  {
    public int NumberOfRows { get; set; }
    public StationOffsetReportDataRows Rows { get; private set; }

    public StationOffsetReportData()
    {
      base.Clear();
      Clear();
    }

    public new void Clear()
    {
      base.Clear();

      Rows = new StationOffsetReportDataRows();
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
      Rows = new StationOffsetReportDataRows(NumberOfRows);
      Rows.Read(reader, NumberOfRows);
      base.Read(reader);
    }
  }
}
