using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using CmvChangeSummaryData = VSS.Productivity3D.WebApi.Models.Compaction.Models.CmvChangeSummaryData;

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

    /// <summary>
    /// Default public constructor.
    /// </summary>
    public CompactionCmvPercentChangeResult()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public CompactionCmvPercentChangeResult(CMVChangeSummaryResult result)
    {
      if (result != null && result.HasData())
      {
        SummaryData = new CmvChangeSummaryData
        {
          Percents = result.Values,
          TotalAreaCoveredSqMeters = result.CoverageArea
        };
      }
    }
  }
}
