using System;
using Newtonsoft.Json;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Models
{
  /// <summary>
  /// The request representation used for a summary CCA request.
  /// </summary>
  public class CCARequest : ProjectID
  {
    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    [JsonProperty(PropertyName = "callId", Required = Required.Default)]
    public Guid? CallId { get; private set; }

    /// <summary>
    /// The lift build settings to use in the request.
    /// </summary>
    [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
    public LiftBuildSettings LiftBuildSettings { get; private set; }

    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult Filter { get; private set; }

    /// <summary>
    /// The filter ID to used in the request.
    /// May be null.
    /// </summary>
    [JsonProperty(PropertyName = "filterID", Required = Required.Default)]
    public long FilterID { get; private set; }

    /// <summary>
    /// Default private constructor
    /// </summary>
    private CCARequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="projectID"></param>
    /// <param name="callId"></param>
    /// <param name="liftBuildSettings"></param>
    /// <param name="filter"></param>
    /// <param name="filterID"></param>
    /// <returns></returns>
    public CCARequest(
      long projectID,
      Guid? callId,
      LiftBuildSettings liftBuildSettings,
      FilterResult filter,
      long filterID
        )
    {
      ProjectId = projectID;
      CallId = callId;
      LiftBuildSettings = liftBuildSettings;
      Filter = filter;
      FilterID = filterID;
    }
    
    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      LiftBuildSettings?.Validate();
      Filter?.Validate();
    }
  }
}
