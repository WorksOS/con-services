using System;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// The result representation of a summary temperature request
  /// </summary>
  public class TemperatureSummaryResult : ContractExecutionResult
  {
    /// <summary>
    /// If the temperature value is constant, this is the minimum constant value of all temperature targets in the processed data.
    /// </summary>
    [JsonProperty(PropertyName = "minimumTemperature")]
    public double MinimumTemperature { get; private set; }

    /// <summary>
    /// If the temperature value is constant, this is the maximum constant value of all temperature targets in the processed data.
    /// </summary>
    [JsonProperty(PropertyName = "maximumTemperature")]
    public double MaximumTemperature { get; private set; }

    /// <summary>
    /// Are the temperature target values applying to all processed cells constant?
    /// </summary>
    [JsonProperty(PropertyName = "isTargetTemperatureConstant")]
    public bool IsTargetTemperatureConstant { get; private set; }

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
    public TemperatureSummaryResult(
      double minimumTemperature,
      double maximumTemperature,
      bool isTargetTemperatureConstant,
      short returnCode,
      double totalAreaCoveredSqMeters,
      double aboveTemperaturePercent,
      double withinTemperaturePercent,
      double belowTemperaturePercent
      )
    {
      MinimumTemperature = minimumTemperature;
      MaximumTemperature = maximumTemperature;
      IsTargetTemperatureConstant = isTargetTemperatureConstant;
      ReturnCode = returnCode;
      TotalAreaCoveredSqMeters = totalAreaCoveredSqMeters;
      AboveTemperaturePercent = aboveTemperaturePercent;
      WithinTemperaturePercent = withinTemperaturePercent;
      BelowTemperaturePercent = belowTemperaturePercent;
    }
  }
}