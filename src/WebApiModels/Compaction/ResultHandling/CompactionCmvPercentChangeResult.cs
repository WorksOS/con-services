using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// Represents result returned by CMV % change request for compaction
  /// </summary>
  public class CompactionCmvPercentChangeResult : ContractExecutionResult
  {
    /// <summary>
    /// The CMV % change data results
    /// </summary>
    [JsonProperty(PropertyName = "cmvChangeData")]
    public CmvChangeSummaryData SummaryData { get; private set; }

    public static CompactionCmvPercentChangeResult CreateEmptyResult() => new CompactionCmvPercentChangeResult();

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private CompactionCmvPercentChangeResult()
    { }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static CompactionCmvPercentChangeResult CreateCmvPercentChangeResult(CMVChangeSummaryResult result)
    {
      if (result == null || !result.HasData())
      {
        return CreateEmptyResult();
      }

      return new CompactionCmvPercentChangeResult
      {
        SummaryData = new CmvChangeSummaryData
        {
          Percents = result.Values,
          TotalAreaCoveredSqMeters = result.CoverageArea
        }
      };
    }
  }
}