using Newtonsoft.Json;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  public class VolumesSummaryData
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