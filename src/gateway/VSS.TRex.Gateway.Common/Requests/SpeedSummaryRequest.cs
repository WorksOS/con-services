using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using VSS.Productivity3D.Models.Models;

namespace VSS.TRex.Gateway.Common.Requests
{
  public class SpeedSummaryRequest : ProjectID
  {
    /// <summary>
    /// The filter instance to use in the request.
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult filter { get; private set; }

    /// <summary>
    /// Sets the machine speed target for Speed Summary requests. During this request Raptor does analysis of all cell passes (filtered out) and searches for the
    /// passes with speed above or below target values. If there is at least one cell pass satisfying the condition - this cell is considered bad.
    /// </summary>
    /// <value>
    /// The machine speed target.
    /// </value>
    [JsonProperty(PropertyName = "machineSpeedTarget", Required = Required.Default)]
    public MachineSpeedTarget machineSpeedTarget { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private SpeedSummaryRequest()
    {
    }

    /// <summary>
    /// Create an instance of the SpeedSummaryRequest class.
    /// </summary>
    public static SpeedSummaryRequest CreateSpeedSummaryRequest(
      Guid projectUid,
      FilterResult filter,
      MachineSpeedTarget machineSpeedTarget
    )
    {
      return new SpeedSummaryRequest
      {
        ProjectUid = projectUid,
        filter = filter,
        machineSpeedTarget = machineSpeedTarget
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      filter?.Validate();

      machineSpeedTarget?.Validate();
    }
  }
}
