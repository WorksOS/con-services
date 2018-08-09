using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling
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

    public static CompactionCmvDetailedResult CreateEmptyResult() => new CompactionCmvDetailedResult();

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private CompactionCmvDetailedResult()
    { }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static CompactionCmvDetailedResult CreateCmvDetailedResult(CMVDetailedResult result)
    {
      if (result == null || !result.HasData())
      {
        return CreateEmptyResult();
      }

      return new CompactionCmvDetailedResult
      {
        Percents = result.Percents
      };
    }
  }
}
