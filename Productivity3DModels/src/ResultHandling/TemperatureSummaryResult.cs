using System;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// The result representation of a summary temperature request
  /// </summary>
  public class TemperatureSummaryResult : TemperatureBaseResult
  {

    /// <summary>
    /// The percentage of the cells that are below the temperature range
    /// </summary>
    [JsonProperty(PropertyName = "belowTemperaturePercent")]
    public double BelowTemperaturePercent { get; private set; }

    /// <summary>
    /// The percentage of cells that are within the target range
    /// </summary>
    [JsonProperty(PropertyName = "withinTemperaturePercent")]
    public double WithinTemperaturePercent { get; private set; }

    /// <summary>
    /// The percentage of the cells that are above the temperature range
    /// </summary>
    [JsonProperty(PropertyName = "aboveTemperaturePercent")]
    public double AboveTemperaturePercent { get; private set; }

    /// <summary>
    /// The internal result code of the request. Documented elsewhere.
    /// </summary>
    [JsonProperty(PropertyName = "returnCode")]
    public short ReturnCode { get; private set; }

    /// <summary>
    /// The total area covered by non-null cells in the request area
    /// </summary>
    [JsonProperty(PropertyName = "totalAreaCoveredSqMeters")]
    public double TotalAreaCoveredSqMeters { get; private set; }

    public bool HasData() => Math.Abs(this.TotalAreaCoveredSqMeters) > 0.001;

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private TemperatureSummaryResult()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="targetData"></param>
    /// <param name="returnCode"></param>
    /// <param name="totalAreaCoveredSqMeters"></param>
    /// <param name="aboveTemperaturePercent"></param>
    /// <param name="withinTemperaturePercent"></param>
    /// <param name="belowTemperaturePercent"></param>
    public TemperatureSummaryResult(
      TemperatureTargetData targetData,
      short returnCode,
      double totalAreaCoveredSqMeters,
      double aboveTemperaturePercent,
      double withinTemperaturePercent,
      double belowTemperaturePercent
      )
    {
      TargetData = targetData;  
      ReturnCode = returnCode;
      TotalAreaCoveredSqMeters = totalAreaCoveredSqMeters;
      AboveTemperaturePercent = aboveTemperaturePercent;
      WithinTemperaturePercent = withinTemperaturePercent;
      BelowTemperaturePercent = belowTemperaturePercent;
    }
  }
}