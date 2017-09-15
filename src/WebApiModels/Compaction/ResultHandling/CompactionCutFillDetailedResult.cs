using Newtonsoft.Json;
using VSS.Common.ResultsHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// Represents result returned by cut-fill details request for compaction.
  /// </summary>
  public class CompactionCutFillDetailedResult : ContractExecutionResult
  {
    /// <summary>
    /// An array of percentages relating to the cut-fill tolerances.
    /// </summary>
    [JsonProperty(PropertyName = "percents")]
    public double[] Percents { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CompactionCutFillDetailedResult()
    { }

    /// <summary>
    /// Creates an instance of the CompactionCutFillDetailedResult class.
    /// </summary>
    /// <param name="result">The Raptor results</param>
    /// <returns>An instance of the CompactionCutFillDetailedResult class.</returns>
    public static CompactionCutFillDetailedResult CreateCutFillDetailedResult(double[] result)
    {
      return new CompactionCutFillDetailedResult
      {
        Percents = result
      };
    }

  }
}
