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
    public FilterResult Filter { get; private set; }

    /// <summary>
    /// The global override value for target pass count range. Optional.
    /// </summary>
    [JsonProperty(PropertyName = "overridingTargetPassCountRange", Required = Required.Default)]
    public TargetPassCountRange OverridingTargetPassCountRange { get; private set; }

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private PassCountSummaryRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public PassCountSummaryRequest(
      Guid projectUid,
      FilterResult filter,
      TargetPassCountRange overridingTargetPassCountRange
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      OverridingTargetPassCountRange = overridingTargetPassCountRange;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      Filter?.Validate();

      OverridingTargetPassCountRange?.Validate();
    }
  }
}
