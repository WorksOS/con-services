using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// Represents result returned by Summary Volumes request
  /// </summary>
  public class CompactionVolumesSummaryResult : ContractExecutionResult
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

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static CompactionVolumesSummaryResult Create(SummaryVolumesResult result, CompactionProjectSettings projectSettings)
    {
      double surplusDeficitVolume = result.Cut - result.Fill;
      double totalVolume = result.Cut + result.Fill;

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

      return new CompactionVolumesSummaryResult
      {
        SummaryVolumeData = new VolumesSummaryData
        {
          Bulking = bulking ?? 0.0,
          Shrinkage = shrinkage ?? 0.0,
          TotalCutVolume = totalCutVolume,
          TotalFillVolume = totalFillVolume,
          TotalMachineCoveragePlanArea = result.TotalCoverageArea,
          TotalVolume = totalVolume,
          NetVolume = surplusDeficitVolume
        },
        Code = result.Code,
        Message = result.Message
      };
    }
  }
}