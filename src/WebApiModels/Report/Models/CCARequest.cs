using System;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;

namespace VSS.Raptor.Service.WebApiModels.Report.Models
{
  /// <summary>
  /// The request representation used for a summary CCA request.
  /// </summary>
  public class CCARequest : ProjectID, IValidatable
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
    public Filter filter { get; private set; }

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
      Filter filter,
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
    /// Create example instance of CCARequest to display in Help documentation.
    /// </summary>
    public new static CCARequest HelpSample
    {
      get
      {
        return new CCARequest()
        {
          projectId = 735,
          callId = null,
          liftBuildSettings = LiftBuildSettings.HelpSample,
          filter = Filter.HelpSample,
          filterID = 0
        };
      }
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();
      if (liftBuildSettings != null)
        liftBuildSettings.Validate();
      if (filter != null)
        filter.Validate();
    }
  }
}