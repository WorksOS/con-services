using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  /// <summary>
  /// The request represents CCA data color palette to be used by a map legend.
  /// </summary>
  /// 
  public class CCAColorPaletteResult : ContractExecutionResult
  {
    /// <summary>
    /// The set of colours to be used by a map legend.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "palettes", Required = Required.Default)]
    public ColorPalette[] palettes { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    /// 
    private CCAColorPaletteResult()
    {
      // ...
    }
    
    /// <summary>
    /// Creates an instance of the CCAColorPaletteResult class.
    /// </summary>
    /// <param name="colorPalettes">A list of color palettes.</param>
    /// <returns>An instance of the CreateCCAColorPaletteResult class.</returns>
    /// 
    public static CCAColorPaletteResult CreateCCAColorPaletteResult(ColorPalette[] colorPalettes)
    {
      return new CCAColorPaletteResult { palettes = colorPalettes };
    }
  }
}
