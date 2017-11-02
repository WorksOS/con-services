using Newtonsoft.Json;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Report.ResultHandling
{
  /// <summary>
  /// Represents result returned by Summary Volumes request
  /// </summary>
  public class CompactionSummaryVolumesResult : ContractExecutionResult
  {
    private CompactionSummaryVolumesResult()
    { }

    [JsonProperty(PropertyName = "volumeSummaryData")]
    public SummaryVolumesData SummaryVolumeData { get; private set; }

    public static CompactionSummaryVolumesResult CreateInstance(SummaryVolumesResult resultObj, CompactionProjectSettings projectSettings)
    {
      double surplusDeficitVolume = resultObj.Cut - resultObj.Fill;
      double totalVolume = resultObj.Cut + resultObj.Fill;

      var totalCutVolume = resultObj.Cut;
      var totalFillVolume = resultObj.Fill;

      if (projectSettings.useDefaultVolumeShrinkageBulking.HasValue && !projectSettings.useDefaultVolumeShrinkageBulking.Value)
      {
        totalCutVolume = resultObj.Cut * (1 + projectSettings.VolumeBulkingPercent / 100);
        totalFillVolume = resultObj.Fill * (1 + projectSettings.VolumeShrinkagePercent / 100);

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

      return new CompactionSummaryVolumesResult
      {
        SummaryVolumeData = new SummaryVolumesData
        {
          Bulking = bulking ?? 0,
          Shrinkage = shrinkage ?? 0,
          TotalCutVolume = totalCutVolume,
          TotalFillVolume = totalFillVolume,
          TotalMachineCoveragePlanArea = resultObj.TotalCoverageArea,
          TotalVolume = totalVolume,
          NetVolume = surplusDeficitVolume
        },
        Code = resultObj.Code,
        Message = resultObj.Message
      };
    }
  }

  public class SummaryVolumesData
  {
    [JsonProperty(PropertyName = "netVolume")]
    public double NetVolume;

    [JsonProperty(PropertyName = "totalVolume")]
    public double TotalVolume;

    [JsonProperty(PropertyName = "totalCutVolume")]
    public double TotalCutVolume;

    [JsonProperty(PropertyName = "totalFillVolume")]
    public double TotalFillVolume;

    [JsonProperty(PropertyName = "totalMachineCoveragePlanArea")]
    public double TotalMachineCoveragePlanArea;

    [JsonProperty(PropertyName = "shrinkage")]
    public double Shrinkage;

    [JsonProperty(PropertyName = "bulking")]
    public double Bulking;
  }
}