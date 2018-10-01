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
    /// The temperature details data results
    /// </summary>
    [JsonProperty(PropertyName = "temperatureDetailsData")]
    public TemperatureDetailsData DetailsData { get; private set; }

    /// <summary>
    /// Default public constructor.
    /// </summary>
    public CompactionTemperatureDetailResult()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public CompactionTemperatureDetailResult(double[] result, double totalArea)
    {
      if (result != null && result.Length > 0 && Math.Abs(totalArea) > 0.001)
      {
        DetailsData = new TemperatureDetailsData
        {
          Percents = result,
          TotalAreaCoveredSqMeters = totalArea
        };
      }
    }
  }
}
