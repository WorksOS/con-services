using ASNodeRaptorReports;

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
    public static GridRow CreateRow(TGridRow report)
    {
      return new GridRow
      {
        Northing = report.Northing,
        Easting = report.Easting,
        Elevation = report.Elevation,
        CutFill = report.CutFill,
        CMV = report.CMV,
        MDP = report.MDP,
        PassCount = report.PassCount,
        Temperature = report.Temperature
      };
    }
  }
}