using ASNodeRaptorReports;
using VSS.Productivity3D.WebApi.Models.Common;

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
        Elevation = report.Elevation != VelociraptorConstants.NULL_SINGLE ? report.Elevation : VelociraptorConstants.NULL_SINGLE,
        CutFill = report.CutFill != VelociraptorConstants.NULL_SINGLE ? report.CutFill: VelociraptorConstants.NULL_SINGLE,
        CMV = report.CMV != VelociraptorConstants.NO_CCV ? (double)report.CMV / 10 : VelociraptorConstants.NO_CCV,
        MDP = report.MDP != VelociraptorConstants.NO_MDP ? (double)report.MDP / 10 : VelociraptorConstants.NO_MDP,
        PassCount = report.PassCount != VelociraptorConstants.NO_PASSCOUNT ? report.PassCount : VelociraptorConstants.NO_PASSCOUNT,
        Temperature = report.Temperature != VelociraptorConstants.NO_TEMPERATURE ? (double)report.Temperature / 10 : VelociraptorConstants.NO_TEMPERATURE
      };
    }
  }
}