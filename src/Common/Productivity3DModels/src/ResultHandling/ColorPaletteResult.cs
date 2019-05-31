using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// The CCA color palette to be used for map tiles and legends
  /// </summary>
  public class ColorPaletteResult : ContractExecutionResult
  {
    /// <summary>
    /// The set of colors to be used by a map legend.
    /// </summary>
    [JsonProperty(PropertyName = "palettes")]
    public ColorPalette[] Palettes { get; set; }

  }
}
