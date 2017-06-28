using System;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApiModels.Report.Models
{
  /// <summary>
  /// The representation of a elevation statistics request
  /// </summary>
  public class ElevationStatisticsRequest : ProjectID, IValidatable
  {
    /// <summary>
    /// Prevents a default instance of the <see cref="SummaryVolumesRequest"/> class from being created.
    /// </summary>
    private ElevationStatisticsRequest()
    {
    }

    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    [JsonProperty(PropertyName = "callId", Required = Required.Default)]
    public Guid? callId { get; protected set; }

    /// <summary>
    /// The filter to be used for the request
    /// </summary>
    [JsonProperty(PropertyName = "Filter", Required = Required.Default)]
    public Filter Filter { get; protected set; }

    /// <summary>
    /// The ID of the filter to be used for the request
    /// </summary>
    [JsonProperty(PropertyName = "FilterID", Required = Required.Default)]
    public long FilterID { get; protected set; }

    /// <summary>
    /// The set of parameters and configuration information relevant to analysis of compaction material layers information for related profile queries.
    /// </summary>
    [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
    public LiftBuildSettings liftBuildSettings { get; protected set; }

    public override void Validate()
    {
      if (this.liftBuildSettings != null)
        this.liftBuildSettings.Validate();

      if (this.Filter != null)
        this.Filter.Validate();
    }

    /// <summary>
    /// Create example instance of Elevation Statistics to display in Help documentation.
    /// </summary>
    public new static ElevationStatisticsRequest HelpSample
    {
      get
      {
        return new ElevationStatisticsRequest
        {
          projectId = 34,
          callId = Guid.NewGuid(),
          Filter = Filter.HelpSample,
          liftBuildSettings = LiftBuildSettings.HelpSample
        };
      }
    }

    /// <summary>
    /// Create instance of ElevationStatisticsRequest
    /// </summary>
    public static ElevationStatisticsRequest CreateElevationStatisticsRequest(
      long projectId,
      Guid? callId,
      Filter filter,
      long filterId,
      LiftBuildSettings liftBuildSettings)
    {
      return new ElevationStatisticsRequest
      {
        projectId = projectId,
        callId = callId,
        Filter = filter,
        FilterID = filterId,
        liftBuildSettings = liftBuildSettings
      };
    }
  }

}



