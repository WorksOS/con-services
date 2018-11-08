using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class SummarySpeedRequest : RequestBase
  {
    /// <summary>
    /// The project to perform the request against
    /// </summary>
    public long projectID { get; set; }

    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    public Guid? callId { get; set; }

    /// <summary>
    /// The filter to be used 
    /// </summary>
    public FilterResult filter { get; set; }

    /// <summary>
    /// The set of parameters and configuration information relevant to analysis of compaction material layers information for related profile queries.
    /// </summary>
    public LiftBuildSettings liftBuildSettings { get; set; }
  }
}
