using System.IO;

namespace VSS.TRex.Reports.Gridded
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
      base.Clear();
      Clear();
    }

    public void Clear()
    {
      Rows = new GriddedReportDataRows();
      NumberOfRows = Rows.Count;
    }

    public void Write(BinaryWriter writer)
    {
      NumberOfRows = Rows.Count;
      writer.Write(NumberOfRows);
      Rows.Write(writer);
      base.Write(writer);
    }

    public void Read(BinaryReader reader)
    {
      NumberOfRows = reader.ReadInt32();
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

