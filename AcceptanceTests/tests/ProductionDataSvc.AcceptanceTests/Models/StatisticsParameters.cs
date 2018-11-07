namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// Request representation for requesting project statistics
  /// </summary>
  public class StatisticsParameters : RequestBase
  {
    /// <summary>
    /// The project to request the statistics for using legacy project id
    /// </summary>
    public long? projectId { get; set; }
    /// <summary>
    /// The project to request the statistics for using uid
    /// </summary>
    public string projectUid { get; set; }

    /// <summary>
    /// The set of surveyed surfaces that should be excluded from the calculation of the spatial and temporal extents of the project.
    /// </summary>
    public long[] excludedSurveyedSurfaceIds { get; set; }
  }
}
