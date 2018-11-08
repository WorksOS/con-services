using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// The representation of a elevation statistics request
  /// </summary>
  public class ElevationStatisticsRequest : RequestBase
  {
    /// <summary>
    /// The project to process the CS definition file into.
    /// </summary>
    /// 
    public long projectId { get; set; }

    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    public Guid? callId { get; set; }

    /// <summary>
    /// The filter to be used for the request
    /// </summary>
    public FilterResult Filter { get; set; }

    /// <summary>
    /// The ID of the filter to be used for the request
    /// </summary>
    public long FilterID { get; set; }

    /// <summary>
    /// The set of parameters and configuration information relevant to analysis of compaction material layers information for related profile queries.
    /// </summary>
    public LiftBuildSettings liftBuildSettings { get; set; }
  }
}
