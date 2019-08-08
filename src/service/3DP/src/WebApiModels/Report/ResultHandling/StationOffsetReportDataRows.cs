using System.Collections.Generic;
using System.IO;

namespace VSS.Productivity3D.WebApi.Models.Report.ResultHandling
{
  public class StationOffsetReportDataRows : List<StationOffsetReportDataRow>
  {
    public StationOffsetReportDataRows()
    {
    }

    public void Write(BinaryWriter writer)
    {
      foreach (var griddedDataRow in this)
      {
        griddedDataRow.Write(writer);
      }
    }

    public void Read(BinaryReader reader, int numberOfRows)
    {
      for (int i = 0; i < numberOfRows; i++)
      {
        var temp = new StationOffsetReportDataRow();
        temp.Read(reader);
        Add(temp);
      }
    }
  }
}
