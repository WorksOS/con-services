using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApiModels.Compaction.Models.Palettes;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// Represents color palettes result for a palettes request
  /// </summary>
  public class CompactionColorPalettesResult : ContractExecutionResult
  {  
    /// <summary>
    /// The palette for displaying CMV detail values.
    /// </summary>
    [JsonProperty(PropertyName = "cmvDetailPalette", Required = Required.Default)]
    public DetailPalette CmvDetailPalette { get; private set; }
    /// <summary>
    /// The palette for displaying pass count detail values.
    /// </summary>
    [JsonProperty(PropertyName = "passCountDetailPalette", Required = Required.Default)]
    public DetailPalette PassCountDetailPalette { get; private set; }
    /// <summary>
    /// The palette for displaying pass count summary values.
    /// </summary>
    [JsonProperty(PropertyName = "passCountSummaryPalette", Required = Required.Default)]
    public SummaryPalette PassCountSummaryPalette { get; private set; }
    /// <summary>
    /// The palette for displaying cut/fill values.
    /// </summary>
    [JsonProperty(PropertyName = "cutFillPalette", Required = Required.Default)]
    public DetailPalette CutFillPalette { get; private set; }
    /// <summary>
    /// The palette for displaying temperature summary values.
    /// </summary>
    [JsonProperty(PropertyName = "temperatureSummaryPalette", Required = Required.Default)]
    public SummaryPalette TemperatureSummaryPalette { get; private set; }
    /// <summary>
    /// The palette for displaying CMV summary values.
    /// </summary>
    [JsonProperty(PropertyName = "cmvSummaryPalette", Required = Required.Default)]
    public SummaryPalette CmvSummaryPalette { get; private set; }
    /// <summary>
    /// The palette for displaying MDP summary values.
    /// </summary>
    [JsonProperty(PropertyName = "mdpSummaryPalette", Required = Required.Default)]
    public SummaryPalette MdpSummaryPalette { get; private set; }
    /// <summary>
    /// The palette for displaying CMV % change values.
    /// </summary>
    [JsonProperty(PropertyName = "cmvPercentChangePalette", Required = Required.Default)]
    public DetailPalette CmvPercentChangePalette { get; private set; }
    /// <summary>
    /// The palette for displaying speed summary values.
    /// </summary>
    [JsonProperty(PropertyName = "speedSummaryPalette", Required = Required.Default)]
    public SummaryPalette SpeedSummaryPalette { get; private set; }
    /// <summary>
    /// The palette for displaying temperature details values.
    /// </summary>
    [JsonProperty(PropertyName = "temperatureDetailPalette", Required = Required.Default)]
    public DetailPalette TemperatureDetailPalette { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    /// 
    private CompactionColorPalettesResult()
    {
      // ...
    }

    /// <summary>
    /// Creates an instance of the CompactionColorPalettesResult class.
    /// </summary>
    /// <returns>An instance of the CompactionColorPalettesResult class.</returns> 
    public static CompactionColorPalettesResult CreateCompactionColorPalettesResult(
      DetailPalette cmvDetailPalette,
      DetailPalette passCountDetailPalette,
      SummaryPalette passCountSummaryPalette,
      DetailPalette cutFillPalette,
      SummaryPalette temperatureSummaryPalette,
      SummaryPalette cmvSummaryPalette,
      SummaryPalette mdpSummaryPalette,
      DetailPalette cmvPercentChangePalette,
      SummaryPalette speedSummaryPalette,
      DetailPalette temperatureDetailPalette)
    {
      return new CompactionColorPalettesResult
      {
        CmvDetailPalette = cmvDetailPalette,
        PassCountDetailPalette = passCountDetailPalette,
        PassCountSummaryPalette = passCountSummaryPalette,
        CutFillPalette = cutFillPalette,
        TemperatureSummaryPalette = temperatureSummaryPalette,
        CmvSummaryPalette = cmvSummaryPalette,
        MdpSummaryPalette = mdpSummaryPalette,
        CmvPercentChangePalette = cmvPercentChangePalette,
        SpeedSummaryPalette = speedSummaryPalette,
        TemperatureDetailPalette = temperatureDetailPalette
      };
    }

   

  }
}
