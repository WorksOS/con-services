using System;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

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
    /// Default public constructor.
    /// </summary>
    public CompactionTemperatureDetailResult()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public CompactionTemperatureDetailResult(double[] result)
    {
      Percents = result;
    }
  }
}
