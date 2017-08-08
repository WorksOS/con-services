using Newtonsoft.Json;
using VLPDDecls;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Utilities;

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

    /// <summary>
    /// Creates CCAColorPaletteResult class sample instance to be displayed in Help documantation.
    /// </summary>
    /// 
    public static CCAColorPaletteResult HelpSample => new CCAColorPaletteResult
    {
      palettes = new[]
      {
        new TColourPalette(CCADataConstants.coverageColors[1], 1),  // 1st pass (Green)...
        new TColourPalette(CCADataConstants.coverageColors[2], 2),  // 2nd pass (Cyan)...
        new TColourPalette(CCADataConstants.coverageColors[3], 3),  // 3rd pass (Red)...
        new TColourPalette(CCADataConstants.coverageColors[4], 4)   // On target (Yellow)...  
      }
    };
  }
}