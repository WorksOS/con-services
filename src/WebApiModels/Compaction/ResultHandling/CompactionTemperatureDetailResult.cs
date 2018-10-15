using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// Represents result returned by Temperature detail request for compaction
  /// </summary>
  public class CompactionTemperatureDetailResult : ContractExecutionResult
  {
    /// <summary>
    /// An array of percentages relating to the temperature targets.
    /// </summary>
    [JsonProperty(PropertyName = "percents")]
    public double[] Percents { get; private set; }

    /// <summary>
    /// Temperature machine target range and whether it is constant or varies.
    /// </summary>
    [JsonProperty(PropertyName = "temperatureTarget")]
    public TemperatureTargetData TemperatureTarget { get; set; }

    /// <summary>
    /// Default public constructor.
    /// </summary>
    public CompactionTemperatureDetailResult()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public CompactionTemperatureDetailResult(double[] result1, TemperatureSummaryResult result2=null)
    {
      if (HasData(result1))
      {
        Percents = result1;
        SetTargets(result2);
      }
    }

    public void SetTargets(TemperatureSummaryResult result)
    {
      if (result != null && result.HasData())
      {
        TemperatureTarget = new TemperatureTargetData
        {
          MinTemperatureMachineTarget = result.TargetData.MinTemperatureMachineTarget / 10,
          MaxTemperatureMachineTarget = result.TargetData.MaxTemperatureMachineTarget / 10,
          TargetVaries = result.TargetData.TargetVaries
        };
      }
    }

    private bool HasData(double[] percents)
    {
      if (percents == null)
        return false;
      return ((IEnumerable<double>)percents).Any<double>((Func<double, bool>)(d => Math.Abs(d) > 0.001));
    }
  }
}
