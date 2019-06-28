using System;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models
{
  public class SpeedSummaryRequest : ProjectID
  {
    /// <summary>
    /// The filter instance to use in the request.
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult Filter { get; private set; }

    /// <summary>
    /// Only MachineSpeedTarget used.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public OverridingTargets Overrides { get; private set; }

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private SpeedSummaryRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public SpeedSummaryRequest(
      Guid? projectUid,
      FilterResult filter,
      MachineSpeedTarget machineSpeedTarget
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      Overrides = new OverridingTargets(machineSpeedTarget: machineSpeedTarget);
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
