using Newtonsoft.Json;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Compaction.ResultHandling
{
  /// <summary>
  /// Represents result returned by CMV Details request for compaction.
  /// </summary>
  public class CompactionCmvDetailedResult : ContractExecutionResult
  {
    /// <summary>
    /// An array of percentages relating to the CMV values encountered in the processed cells.
    /// The percentages are for CMV values between the minimum and target, on target, between the target and the maximum and above the maximum CMV.
    /// </summary>
    [JsonProperty(PropertyName = "percents")]
    public double[] Percents { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CompactionCmvDetailedResult()
    {
      // ...
    }

    /// <summary>
    /// Creates an instance of the CompactionCmvDetailedResult class.
    /// </summary>
    /// <param name="result">An instance of the CMVDetailedResult class.</param>
    /// <returns>An instance of the CompactionCmvDetailedResult class.</returns>
    public static CompactionCmvDetailedResult CreateCmvDetailedResult(CMVDetailedResult result)
    {
      return new CompactionCmvDetailedResult
      {
        Percents = result.percents
      };
    }

  }
}
