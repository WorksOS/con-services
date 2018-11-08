using System;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports
{
  /// <summary>
  /// Defines all the grid report production data values that are returned from Raptor.
  /// </summary>
  public class StationOffsetReport : ICompactionReport
  {
    /// <summary>
    /// The report's 'start' time from a time based filter.
    /// </summary>
    public DateTime? StartTime { get; private set; }

    /// <summary>
    /// The report's 'end' time from a time based filter.
    /// </summary>
    public DateTime? EndTime { get; private set; }

    /// <summary>
    /// Grid report rows
    /// </summary>
    public StationRow[] Rows { get; private set; }

    public bool ElevationReport { get; private set; }
    public bool CutFillReport { get; private set; }
    public bool CmvReport { get; private set; }
    public bool MdpReport { get; private set; }
    public bool PassCountReport { get; private set; }
    public bool TemperatureReport { get; private set; }

    /// <summary>
    /// Creates an instance of the GridReport class.
    /// </summary>
    /// <param name="startTime">The report's 'start' time.</param>
    /// <param name="endTime">The report's 'end' time.</param>
    /// <param name="rows">Grid rows.</param>
    /// <param name="request"></param>
    /// <returns>An instance of the GridReport class.</returns>
    public static StationOffsetReport CreateReport(DateTime startTime, DateTime endTime, StationRow[] rows, CompactionReportStationOffsetRequest request)
    {
      var report = new StationOffsetReport
      {
        Rows = rows,
        TemperatureReport = request.ReportTemperature,
        CmvReport = request.ReportCMV,
        CutFillReport = request.ReportCutFill,
        ElevationReport = request.ReportElevation,
        MdpReport = request.ReportMDP,
        PassCountReport = request.ReportPassCount,
        StartTime = startTime,
        EndTime = endTime
      };

      return report;
    }
  }
}
