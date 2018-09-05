using System;
using Newtonsoft.Json;
using VSS.Productivity3D.Models.Models;

namespace VSS.TRex.Gateway.Common.Requests
{
  /// <summary>
  /// The request representation used to request Pass Count summary.
  /// </summary>
  public class PassCountSummaryRequest : ProjectID
  {
    /// <summary>
    /// The filter instance to use in the request.
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult filter { get; private set; }

    /// <summary>
    /// The global override value for target pass count range. Optional.
    /// </summary>
    [JsonProperty(PropertyName = "overridingTargetPassCountRange", Required = Required.Default)]
    public TargetPassCountRange overridingTargetPassCountRange { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private PassCountSummaryRequest()
    {
    }

    /// <summary>
    /// Create an instance of the PassCountSummaryRequest class.
    /// </summary>
    public static PassCountSummaryRequest CreatePassCountSummaryRequest(
      Guid projectUid,
      FilterResult filter,
      TargetPassCountRange overridingTargetPassCountRange
    )
    {
      return new PassCountSummaryRequest
      {
        ProjectUid = projectUid,
        filter = filter,
        overridingTargetPassCountRange = overridingTargetPassCountRange
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      filter?.Validate();

      overridingTargetPassCountRange?.Validate();
    }
  }
}
