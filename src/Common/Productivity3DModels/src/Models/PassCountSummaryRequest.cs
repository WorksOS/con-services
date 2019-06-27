using System;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models
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
    /// Only TargetPassCountRange used.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public OverridingTargets Overrides { get; private set; }


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
      Guid? projectUid,
      FilterResult filter,
      TargetPassCountRange overridingTargetPassCountRange
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      Overrides = new OverridingTargets(overridingTargetPassCountRange: overridingTargetPassCountRange);
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      Filter?.Validate();
      Overrides?.Validate();
    }
  }
}
