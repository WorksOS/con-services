using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling
{
  public class ProgressiveSummaryVolumesResult : ContractExecutionResult
  {
    /// <summary>
    /// The collection of data/volume pairs for all computed intervals in the progressive volume response
    /// </summary>
    [JsonProperty("volumes")]
    public ProgressiveSummaryVolumesResultItem[] Volumes { get; private set; }

    public static ProgressiveSummaryVolumesResult Create(ProgressiveSummaryVolumesResultItem[] volumes)
    {
      return new ProgressiveSummaryVolumesResult {Volumes = volumes};
    }
  }
}
