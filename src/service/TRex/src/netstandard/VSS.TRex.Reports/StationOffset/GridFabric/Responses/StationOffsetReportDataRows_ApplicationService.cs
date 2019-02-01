using System.Collections.Generic;
using System.IO;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Responses
{
  /// <summary>
  /// Contains the prepared result for the client to consume
  /// </summary>
  public class StationOffsetReportDataRows_ApplicationService : List<StationOffsetReportDataRow_ApplicationService>
  {
    public StationOffsetReportDataRows_ApplicationService()
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
        var temp = new StationOffsetReportDataRow_ApplicationService();
        temp.Read(reader);
        Add(temp);
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
