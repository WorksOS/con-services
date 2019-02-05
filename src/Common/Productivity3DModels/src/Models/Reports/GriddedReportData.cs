using System.IO;

namespace VSS.Productivity3D.Models.Models.Reports
{
  /// <summary>
  /// Contains the prepared result for the client to consume
  /// </summary>
  public class GriddedReportData : ReportTypeData
  {
    public int NumberOfRows { get; set; }
    public GriddedReportDataRows Rows { get; set; }
    
    public GriddedReportData()
    {
      Clear();
    }

    public new void Clear()
    {
      base.Clear();

      Rows = new GriddedReportDataRows();
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
      Rows.Read(reader, NumberOfRows);
      base.Read(reader);
    }
  }
}

