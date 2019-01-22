using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// The result representation of a summary CCA request
  /// </summary>
  public class CCASummaryResult : ContractExecutionResult
  {
    /// <summary>
    /// The percentage of cells that are complete within the target bounds
    /// </summary>
    [JsonProperty(PropertyName = "completePercent")]
    public double CompletePercent { get; private set; }

    /// <summary>
    /// The percentage of the cells that are over-complete
    /// </summary>
    [JsonProperty(PropertyName = "overCompletePercent")]
    public double OverCompletePercent { get; private set; }

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

    /// <summary>
    /// The percentage of the cells that are under complete
    /// </summary>
    [JsonProperty(PropertyName = "underCompletePercent")]
    public double UnderCompletePercent { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CCASummaryResult()
    { }

    /// <summary>
    /// Create instance of CCASummaryResult
    /// </summary>
    public static CCASummaryResult Create(
      double completePercent,
      double overCompactedPercent,
      short returnCode,
      double totalAreaCoveredSqMeters,
      double underCompactedPercent
      )
    {
      return new CCASummaryResult
      {
        CompletePercent = completePercent,
        OverCompletePercent = overCompactedPercent,
        ReturnCode = returnCode,
        TotalAreaCoveredSqMeters = totalAreaCoveredSqMeters,
        UnderCompletePercent = underCompactedPercent
      };
    }
  }
}