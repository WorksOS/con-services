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
    /// Sets the machine speed target for Speed Summary requests. During this request Raptor does analysis of all cell passes (filtered out) and searches for the
    /// passes with speed above or below target values. If there is at least one cell pass satisfying the condition - this cell is considered bad.
    /// </summary>
    /// <value>
    /// The machine speed target.
    /// </value>
    [JsonProperty(PropertyName = "machineSpeedTarget", Required = Required.Default)]
    public MachineSpeedTarget MachineSpeedTarget { get; private set; }

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
      MachineSpeedTarget = machineSpeedTarget;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      Filter?.Validate();

      MachineSpeedTarget?.Validate();
    }
  }
}
