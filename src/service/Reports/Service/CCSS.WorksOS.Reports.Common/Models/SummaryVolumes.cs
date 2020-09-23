using Newtonsoft.Json;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class SummaryVolumes : SummaryDataBase
  {
    [JsonProperty(PropertyName = "volumeSummaryData")]
    public SummaryVolumesData SummaryVolumeData { get; private set; }
  }

  public class SummaryVolumesData
  {
    [JsonProperty(PropertyName = "netVolume")]
    public double NetVolume { get; set; }

    [JsonProperty(PropertyName = "totalVolume")]
    public double TotalVolume { get; set; }

    [JsonProperty(PropertyName = "totalCutVolume")]
    public double TotalCutVolume { get; set; }

    [JsonProperty(PropertyName = "totalFillVolume")]
    public double TotalFillVolume { get; set; }

    [JsonProperty(PropertyName = "totalMachineCoveragePlanArea")]
    public double TotalMachineCoveragePlanArea { get; set; }

    [JsonProperty(PropertyName = "shrinkage")]
    public double Shrinkage { get; set; }

    [JsonProperty(PropertyName = "bulking")]
    public double Bulking { get; set; }

    [JsonIgnore]
    public bool IsEmpty => NetVolume == 0 && TotalVolume == 0 && TotalCutVolume == 0 && TotalFillVolume == 0;
  }
}

