using Newtonsoft.Json;
using VLPDDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;

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
    public TColourPalette[] palettes { get; private set; }

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
    public static CCAColorPaletteResult CreateCCAColorPaletteResult(TColourPalette[] colorPalettes)
    {
      return new CCAColorPaletteResult { palettes = colorPalettes };
    }
  }
}