using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// Base class for TRex summary requests
  /// </summary>
  public abstract class TRexSummaryRequest : ProjectID
  {
    /// <summary>
    /// The filter instance to use in the request.
    /// Value may be null.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public FilterResult Filter { get; set; }

    /// <summary>
    /// Only TargetPassCountRange used.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public OverridingTargets Overrides { get; set; }

    /// <summary>
    /// Settings for lift analysis
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public LiftSettings LiftSettings { get; set; }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      Filter?.Validate();
      Overrides?.Validate();
      LiftSettings?.Validate();
    }

  }
}
