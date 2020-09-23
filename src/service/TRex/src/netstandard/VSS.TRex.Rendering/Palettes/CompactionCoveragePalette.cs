using System.Drawing;
using VSS.TRex.Types.CellPasses;

namespace VSS.TRex.Rendering.Palettes
{
  /// <summary>
  /// Simple palette for rendering raw CompactionCoveragePalette data (0=hasCMVData; 1 = hasNoCMVData)
  /// </summary>
  public class CompactionCoveragePalette : PaletteBase
  {
    public Color HasCMVData = Color.Green;
    public Color HasNoCMVData = Color.Cyan;

   
    public CompactionCoveragePalette() : base(null)
    {
    }
    
    /// <summary>
    /// Choose the appropriate colour from the palette presence of CMV value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public Color ChooseColour(short value)
    {
      if (value == CellPassConsts.NullCCV)
        return HasNoCMVData;
      return HasCMVData;
    }
  }
}
