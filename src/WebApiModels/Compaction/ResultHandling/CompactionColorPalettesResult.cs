
using System.Collections.Generic;
using Newtonsoft.Json;
using VLPDDecls;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Models;


namespace VSS.Raptor.Service.WebApiModels.Compaction.ResultHandling
{
  /// <summary>
  /// Represents color palettes result for a palettes request
  /// </summary>
  public class CompactionColorPalettesResult : ContractExecutionResult
  {
    /// <summary>
    /// The set of colours to be used by a map legend for each display type.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "palettes", Required = Required.Default)]
    public List<Palette> palettes { get; private set; }

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
    /// <param name="palettes">A list of color palettes.</param>
    /// <returns>An instance of the CompactionColorPalettesResult class.</returns>
    /// 
    public static CompactionColorPalettesResult CreateCompactionColorPalettesResult(List<Palette> palettes)
    {
      return new CompactionColorPalettesResult() { palettes = palettes };
    }

    /// <summary>
    /// The representation of a palette for a display type
    /// </summary>
    public class Palette
    {
      /// <summary>
      /// THe display type of the palette
      /// </summary>
      public DisplayMode displayMode { get; set; }
      /// <summary>
      /// The colors for the palette
      /// </summary>
      public TColourPalette[] colors { get; set; }
    }

  }
}
