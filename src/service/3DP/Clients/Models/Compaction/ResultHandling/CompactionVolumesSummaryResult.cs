using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling
{
  /// <summary>
  /// Represents result returned by Summary Volumes request
  /// </summary>
  public class CompactionVolumesSummaryResult : ContractExecutionResult, IMasterDataModel
  {
    [JsonProperty(PropertyName = "volumeSummaryData")]
    public VolumesSummaryData SummaryVolumeData { get; private set; }

    public static CompactionVolumesSummaryResult CreateEmptyResult() => new CompactionVolumesSummaryResult();

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private CompactionVolumesSummaryResult()
    { }


    public CompactionVolumesSummaryResult(int errorCode, string errorMessage)
      : base(errorCode, errorMessage)
    { }

    public static VolumesSummaryData Convert(SummaryVolumesResult result, CompactionProjectSettings projectSettings)
    {
      var surplusDeficitVolume = result.Cut - result.Fill;
      var totalVolume = result.Cut + result.Fill;

      var totalCutVolume = result.Cut;
      var totalFillVolume = result.Fill;

      if (projectSettings.useDefaultVolumeShrinkageBulking.HasValue && !projectSettings.useDefaultVolumeShrinkageBulking.Value)
      {
        totalCutVolume = result.Cut * (1 + projectSettings.VolumeBulkingPercent / 100);
        totalFillVolume = result.Fill * (1 + projectSettings.VolumeShrinkagePercent / 100);

        surplusDeficitVolume = totalCutVolume - totalFillVolume;
        totalVolume = totalCutVolume + totalFillVolume;
      }

      double? bulking;
      double? shrinkage;

      if (projectSettings.useDefaultVolumeShrinkageBulking.HasValue && projectSettings.useDefaultVolumeShrinkageBulking.Value)
      {
        bulking = projectSettings.VolumeBulkingPercent;
        shrinkage = projectSettings.VolumeShrinkagePercent;
      }
      else
      {
        bulking = projectSettings.customBulkingPercent;
        shrinkage = projectSettings.customShrinkagePercent;
      }

      return new VolumesSummaryData
      {
        Bulking = bulking ?? 0.0,
        Shrinkage = shrinkage ?? 0.0,
        TotalCutVolume = totalCutVolume,
        TotalFillVolume = totalFillVolume,
        TotalMachineCoveragePlanArea = result.TotalCoverageArea,
        TotalVolume = totalVolume,
        NetVolume = surplusDeficitVolume
      };
    }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static CompactionVolumesSummaryResult Create(SummaryVolumesResult result, CompactionProjectSettings projectSettings)
    {
      return new CompactionVolumesSummaryResult
      {
        SummaryVolumeData = Convert(result, projectSettings),
        Code = result.Code,
        Message = result.Message
      };
    }

    public List<string> GetIdentifiers() => new List<string>();
  }
}
