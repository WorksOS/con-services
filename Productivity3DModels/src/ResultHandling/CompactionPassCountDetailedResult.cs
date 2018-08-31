using System.Linq;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// Represents result returned by Pass Count Details request for compaction.
  /// </summary>
  public class CompactionPassCountDetailedResult : ContractExecutionResult
  {
    [JsonProperty(PropertyName = "passCountDetailsData")]
    public PassCountDetailsData DetailedData { get; private set; }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionPassCountDetailedResult()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="result"></param>
    public CompactionPassCountDetailedResult(PassCountDetailedResult result)
    {
      if (result != null && result.HasData())
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
        };
      }
    }
  }
}