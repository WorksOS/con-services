
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.WebApiModels.Report.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.Compaction.ResultHandling
{
  /// <summary>
  /// Represents result returned by temperature Summary request for compaction
  /// </summary>
  public class CompactionTemperatureSummaryResult : ContractExecutionResult
  {
    /// <summary>
    /// The temperature summary data results
    /// </summary>
    [JsonProperty(PropertyName = "temperatureSummaryData")]
    public TemperatureSummaryData SummaryData { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CompactionTemperatureSummaryResult()
    { }


    /// <summary>
    ///TemperatureSummaryResult create instance
    /// </summary>
    /// <param name="result"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static CompactionTemperatureSummaryResult CreateTemperatureSummaryResult(TemperatureSummaryResult result)
    {
      var temperatureResult = new CompactionTemperatureSummaryResult
      {
        SummaryData = new TemperatureSummaryData
        {
          PercentEqualsTarget = result.withinTemperaturePercent,
          PercentGreaterThanTarget = result.aboveTemperaturePercent,
          PercentLessThanTarget = result.belowTemperaturePercent,
          TotalAreaCoveredSqMeters = result.totalAreaCoveredSqMeters,
          TemperatureTarget = new TemperatureTargetData
          {
            MinTemperatureMachineTarget = result.minimumTemperature / 10,
            MaxTemperatureMachineTarget = result.maximumTemperature / 10,
            TargetVaries = !result.isTargetTemperatureConstant
          }
        }
      };
      return temperatureResult;
    }

    /// <summary>
    /// Temperature summary data returned
    /// </summary>
    public class TemperatureSummaryData
    {
      /// <summary>
      /// The percentage of cells that are compacted within the target bounds
      /// </summary>
      [JsonProperty(PropertyName = "percentEqualsTarget")]
      public double PercentEqualsTarget { get; set; }
      /// <summary>
      /// The percentage of the cells that are over-compacted
      /// </summary>
      [JsonProperty(PropertyName = "percentGreaterThanTarget")]
      public double PercentGreaterThanTarget { get; set; }
      /// <summary>
      /// The percentage of the cells that are under compacted
      /// </summary>
      [JsonProperty(PropertyName = "percentLessThanTarget")]
      public double PercentLessThanTarget { get; set; }
      /// <summary>
      /// The total area covered by non-null cells in the request area
      /// </summary>
      [JsonProperty(PropertyName = "totalAreaCoveredSqMeters")]
      public double TotalAreaCoveredSqMeters { get; set; }
      /// <summary>
      /// Temperature machine target range and whether it is constant or varies.
      /// </summary>
      [JsonProperty(PropertyName = "temperatureTarget")]
      public TemperatureTargetData TemperatureTarget { get; set; }

    }

    /// <summary>
    /// Temperature target data returned
    /// </summary>
    public class TemperatureTargetData
    {
      /// <summary>
      /// If the Temperature range is constant, this is the minimum constant value of all temperature target ranges in the processed data.
      /// </summary>
      [JsonProperty(PropertyName = "minTemperatureMachineTarget")]
      public double MinTemperatureMachineTarget { get; set; }
      /// <summary>
      /// If the Temperature range is constant, this is the maximum constant value of all temperature target ranges in the processed data.
      /// </summary>
      [JsonProperty(PropertyName = "maxTemperatureMachineTarget")]
      public double MaxTemperatureMachineTarget { get; set; }
      /// <summary>
      /// Are the temperature target ranges applying to all processed cells varying?
      /// </summary>
      [JsonProperty(PropertyName = "targetVaries")]
      public bool TargetVaries { get; set; }
    }
  }
}
