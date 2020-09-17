using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling
{
  public class CompactionProgressiveVolumesSummaryResult : ContractExecutionResult, IMasterDataModel
  {
    [JsonProperty(PropertyName = "volumes")]
    public ProgressiveCompactionVolumesSummaryDataItem[] Volumes { get; private set; }

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private CompactionProgressiveVolumesSummaryResult()
    {
    }

    public CompactionProgressiveVolumesSummaryResult(int errorCode, string errorMessage)
      : base(errorCode, errorMessage)
    {
    }

    public List<string> GetIdentifiers() => new List<string>();

    public static CompactionProgressiveVolumesSummaryResult Create(ProgressiveSummaryVolumesResult result, CompactionProjectSettings projectSettings)
    {
      return new CompactionProgressiveVolumesSummaryResult
      {
        Volumes = result.Volumes.Select(x => ProgressiveCompactionVolumesSummaryDataItem.Create(x.Date,
          CompactionVolumesSummaryResult.Convert(x.Volume, projectSettings))).ToArray(),
        Code = result.Code,
        Message = result.Message
      };
    }
  }
}
