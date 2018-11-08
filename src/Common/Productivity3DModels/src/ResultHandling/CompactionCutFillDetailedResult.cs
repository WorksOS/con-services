using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling
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
    /// Default private constructor.
    /// </summary>
    private CompactionCutFillDetailedResult()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="result"></param>
    public CompactionCutFillDetailedResult(double[] result)
    {
      Percents = result;
    }
  }
}