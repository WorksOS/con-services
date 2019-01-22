using ASNodeRaptorReports;
using VSS.Productivity3D.Models.Models.Reports;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports
{
  public class StationOffsetRow : ReportRowBase
  {
    public double Offset { get; private set; }
    public double Station { get; private set; }

    /// <summary>
    /// Static constructor for the <see cref="StationOffsetRow"/> type.
    /// </summary>
    /// <returns>Returns an instance of <see cref="StationOffsetRow"/> populated from the supplied <see cref="TStationOffset"/> object.</returns>
    public static StationOffsetRow CreateRow(TStationOffset offset, CompactionReportRequest request)
    {
      var row = new StationOffsetRow
      {
        Offset = offset.Position,
        Station = offset.Station,
      };
      row.SetValues(offset.Northing, offset.Easting, offset.Elevation, offset.CutFill, offset.CMV, offset.MDP, offset.PassCount, offset.Temperature);
      row.SetReportFlags(request);
      return row;      
    }
  }
}
