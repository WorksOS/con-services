using Newtonsoft.Json;
using System.Linq;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// Represents result returned by Pass Count Details request for compaction.
  /// </summary>
  public class CompactionPassCountDetailedResult : ContractExecutionResult
  {
    [JsonProperty(PropertyName = "passCountDetailsData")]
    public PassCountDetailsData DetailedData { get; private set; }

    public static CompactionPassCountDetailedResult CreateEmptyResult() => new CompactionPassCountDetailedResult();

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private CompactionPassCountDetailedResult()
    { }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static CompactionPassCountDetailedResult CreatePassCountDetailedResult(PassCountDetailedResult result)
    {
      if (result == null || !result.HasData())
      {
        return CreateEmptyResult();
      }

      return new CompactionPassCountDetailedResult
      {
        DetailedData = new PassCountDetailsData
        {
          Percents = result.Percents.Skip(1).ToArray(), //don't return the pass count 0 value (see PassCountSettings)
          PassCountTarget = new PassCountTargetData
          {
            MinPassCountMachineTarget = result.ConstantTargetPassCountRange.min,
            MaxPassCountMachineTarget = result.ConstantTargetPassCountRange.max,
            TargetVaries = !result.IsTargetPassCountConstant
          },
          TotalCoverageArea = result.TotalCoverageArea
        }
      };
    }
  }
}