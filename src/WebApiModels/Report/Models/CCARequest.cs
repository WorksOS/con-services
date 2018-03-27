using Newtonsoft.Json;
using System;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApiModels.Report.Models
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
    public Guid? callId { get; private set; }

    /// <summary>
    /// The lift build settings to use in the request.
    /// </summary>
    [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
    public LiftBuildSettings liftBuildSettings { get; private set; }

    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult filter { get; private set; }

    /// <summary>
    /// The filter ID to used in the request.
    /// May be null.
    /// </summary>
    [JsonProperty(PropertyName = "filterID", Required = Required.Default)]
    public long filterID { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CCARequest()
    {
    }

    /// <summary>
    /// Create instance of CCARequest
    /// </summary>
    public static CCARequest CreateCCARequest(
      long projectID,
      Guid? callId,
      LiftBuildSettings liftBuildSettings,
      FilterResult filter,
      long filterID
        )
    {
      return new CCARequest
      {
        projectId = projectID,
        callId = callId,
        liftBuildSettings = liftBuildSettings,
        filter = filter,
        filterID = filterID
      };
    }
    
    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      liftBuildSettings?.Validate();
      filter?.Validate();
    }
  }
}