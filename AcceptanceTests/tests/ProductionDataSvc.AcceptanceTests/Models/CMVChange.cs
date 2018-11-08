using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// Represents speed summary request.
  /// </summary>
  public class CMVChangeSummaryRequest : RequestBase
  {
    /// <summary>
    /// The project to process the CS definition file into.
    /// </summary>
    public long projectId { get; set; }

    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    public Guid? callId { get; set; }

    /// <summary>
    /// The filter to be used 
    /// </summary>
    public FilterResult filter { get; set; }

    /// <summary>
    /// Gets or sets the filter identifier.
    /// </summary>
    /// <value>
    /// The filter identifier.
    /// </value>
    public int filterId { get; set; }

    /// <summary>
    /// The set of parameters and configuration information relevant to analysis of compaction material layers information for related profile queries.
    /// </summary>
    public LiftBuildSettings liftBuildSettings { get; set; }

    /// <summary>
    /// Sets the CMV change summary values to compare against.
    /// </summary>
    public double[] CMVChangeSummaryValues { get; set; }
  }
}
