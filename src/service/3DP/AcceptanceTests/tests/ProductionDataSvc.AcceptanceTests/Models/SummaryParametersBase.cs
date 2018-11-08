using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// Represents typics request for summary reports. Currently include Summary Volumes and Summary Thickness.
  /// </summary>
  public class SummaryParametersBase : RequestBase
  {
    /// <summary>
    /// The project to perform the request against
    /// </summary>
    public long? projectID { get; set; }

    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    public Guid? callId { get; set; }

    /// <summary>
    /// The base or earliest filter to be used for filter-filter and filter-design volumes.
    /// </summary>
    public FilterResult baseFilter { get; set; }

    /// <summary>
    /// The ID of the base or earliest filter to be used for filter-filter and filter-design volumes.
    /// </summary>
    public long baseFilterID { get; set; }

    /// <summary>
    /// The top or latest filter to be used for filter-filter and design-filter volumes
    /// </summary>
    public FilterResult topFilter { get; set; }

    /// <summary>
    /// The ID of the top or latest filter to be used for filter-filter and design-filter volumes
    /// </summary>
    public long topFilterID { get; set; }

    /// <summary>
    /// An additional spatial constraining filter that may be used to provide additional control over the area the summary volumes are being calculated over.
    /// </summary>
    public FilterResult additionalSpatialFilter { get; set; }

    /// <summary>
    /// The ID of an additional spatial constraining filter that may be used to provide additional control over the area the summary volumes are being calculated over.
    /// </summary>
    public long additionalSpatialFilterID { get; set; }

    /// <summary>
    /// The set of parameters and configuration information relevant to analysis of compaction material layers information for related profile queries.
    /// </summary>
    public LiftBuildSettings liftBuildSettings { get; set; }
  }
}
