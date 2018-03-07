using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApiModels.Compaction.Models.Palettes;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// Represents color palette result for a elevation palette request
  /// </summary>
  public class CompactionDetailPaletteResult : ContractExecutionResult
  {
    /// <summary>
    /// The palette for displaying elevation values.
    /// </summary>
    [JsonProperty(PropertyName = "palette", Required = Required.Default)]
    public DetailPalette Palette { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    /// 
    private CompactionDetailPaletteResult()
    { }

    /// <summary>
    /// Creates an instance of the CompactionDetailPaletteResult class.
    /// </summary>
    /// <returns>An instance of the CompactionDetailPaletteResult class.</returns> 
    public static CompactionDetailPaletteResult CreateCompactionDetailPaletteResult(
      DetailPalette elevationPalette)
    {
      return new CompactionDetailPaletteResult
      {
        Palette = elevationPalette,
        Message = elevationPalette == null ? "No elevation range" : DefaultMessage,
        Code = elevationPalette == null ? ContractExecutionStatesEnum.FailedToGetResults : ContractExecutionStatesEnum.ExecutedSuccessfully
      };
    }
  }
}