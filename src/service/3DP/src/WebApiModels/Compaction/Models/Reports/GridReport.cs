using Newtonsoft.Json;
using System;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports
{
  /// <summary>
  /// Defines all the grid report production data values that are returned from Raptor.
  /// </summary>
  /// 
  public class GridReport : ICompactionReport
  {
    /// <summary>
    /// The report's 'start' time from a time based filter.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public DateTime? StartTime { get; private set; }

    /// <summary>
    /// The report's 'end' time from a time based filter.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public DateTime? EndTime { get; private set; }

    /// <summary>
    /// Grid report rows
    /// </summary>
    /// 
    [JsonProperty(Required = Required.Default)]
    public GridRow[] Rows { get; private set; }

    /// <summary>
    /// Creates an instance of the GridReport class.
    /// </summary>
    /// <param name="rows">Grid rows.</param>
    /// <param name="startTime">The report's 'start' time.</param>
    /// <param name="endTime">The report's 'end' time.</param>
    /// <returns>An instance of the GridReport class.</returns>
    /// 
    public static GridReport CreateGridReport(DateTime startTime, DateTime endTime, GridRow[] rows)
    {
      return new GridReport { StartTime = startTime, EndTime = endTime, Rows = rows };
    }
  }
}