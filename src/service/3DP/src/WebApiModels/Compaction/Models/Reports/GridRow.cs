#if RAPTOR
using ASNodeRaptorReports;
#endif
using VSS.Productivity3D.Models.Models.Reports;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports
{
  /// <summary>
  /// Defines a grid report row.
  /// </summary>
  public class GridRow : ReportRowBase
  {
    /// <summary>
    /// Create an instance of the <see cref="GridRow"/> class.
    /// </summary>
    /// <returns>An instance of the <see cref="GridRow"/> class.</returns>
#if RAPTOR
    public static GridRow CreateRow(TGridRow report, CompactionReportRequest request)
    {
      var row = new GridRow();
      row.SetValues(report.Northing, report.Easting, report.Elevation, report.CutFill, report.CMV, report.MDP, report.PassCount, report.Temperature);
      row.SetReportFlags(request);
      return row;
    }
#endif
    public static GridRow CreateRow(GriddedReportDataRow dataRow, CompactionReportRequest request)
    {
      var row = new GridRow();
      row.SetValues(dataRow.Northing, dataRow.Easting, dataRow.Elevation, dataRow.CutFill, dataRow.Cmv, dataRow.Mdp, dataRow.PassCount, dataRow.Temperature);
      row.SetReportFlags(request);
      return row;
    }

  }
}
