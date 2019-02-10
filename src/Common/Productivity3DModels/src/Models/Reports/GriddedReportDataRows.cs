using System.Collections.Generic;
using System.IO;

namespace VSS.Productivity3D.Models.Models.Reports
{
  /// <summary>
  /// Contains the prepared result for the client to consume
  /// </summary>
  public class GriddedReportDataRows : List<GriddedReportDataRowBase>
  {
    public GriddedReportDataRows()
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
        var grdr = new GriddedReportDataRowBase();
        grdr.Read(reader);
        Add(grdr);
      }
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
