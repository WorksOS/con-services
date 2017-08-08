
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;

namespace VSS.Productivity3D.WebApiModels.Report.ResultHandling
{
  /// <summary>
  /// The result representation of a summary temperature request
  /// </summary>
  public class TemperatureSummaryResult : ContractExecutionResult
  {
    /// <summary>
    /// If the temperature value is constant, this is the minimum constant value of all temperature targets in the processed data.
    /// </summary>
    public double minimumTemperature { get; private set; }

    /// <summary>
    /// If the temperature value is constant, this is the maximum constant value of all temperature targets in the processed data.
    /// </summary>
    public double maximumTemperature { get; private set; }

    /// <summary>
    /// Are the temperature target values applying to all processed cells constant?
    /// </summary>
    public bool isTargetTemperatureConstant { get; private set; }

    /// <summary>
    /// The percentage of the cells that are below the temperature range
    /// </summary>
    public double belowTemperaturePercent { get; private set; }
    /// <summary>
    /// The percentage of cells that are within the target range
    /// </summary>
    public double withinTemperaturePercent { get; private set; }

    /// <summary>
    /// The percentage of the cells that are above the temperature range
    /// </summary>
    public double aboveTemperaturePercent { get; private set; }

    /// <summary>
    /// The internal result code of the request. Documented elsewhere.
    /// </summary>
    public short returnCode { get; private set; }

    /// <summary>
    /// The total area covered by non-null cells in the request area
    /// </summary>
    public double totalAreaCoveredSqMeters { get; private set; }


    /// <summary>
    /// Private constructor
    /// </summary>
    private TemperatureSummaryResult()
    { }

    /// <summary>
    /// Create instance of CMVSummaryResult
    /// </summary>
    public static TemperatureSummaryResult CreateTemperatureSummaryResult(
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
      return new TemperatureSummaryResult
      {
        minimumTemperature = minimumTemperature,
        maximumTemperature = maximumTemperature,
        isTargetTemperatureConstant = isTargetTemperatureConstant,
        returnCode = returnCode,
        totalAreaCoveredSqMeters = totalAreaCoveredSqMeters,
        aboveTemperaturePercent = aboveTemperaturePercent,
        withinTemperaturePercent = withinTemperaturePercent,
        belowTemperaturePercent = belowTemperaturePercent
      };
    }
  }
}
