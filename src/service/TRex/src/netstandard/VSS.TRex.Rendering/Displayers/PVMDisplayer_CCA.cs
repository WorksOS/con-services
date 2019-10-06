using System.Drawing;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Rendering.Palettes.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for CCA information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_CCA : PVMDisplayerBase
  {
    protected override void SetSubGrid(ISubGrid value)
    {
      base.SetSubGrid(value);

      if (SubGrid != null)
        CastRequestObjectTo<ClientCCALeafSubGrid>(SubGrid, ThrowTRexClientLeafSubGridTypeCastException<ClientCCALeafSubGrid>);
    }

    protected override void SetPalette(IPlanViewPalette value)
    {
      base.SetPalette(value);

      if (Palette != null)
        CastRequestObjectTo<CCAPalette>(Palette, ThrowTRexColorPaletteTypeCastException<CCAPalette>);
    }

    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      const byte HALF_PASS_FACTOR = 2;

      var cellValue = ((ClientCCALeafSubGrid)SubGrid).Cells[east_col, north_row];

      if (cellValue.MeasuredCCA == CellPassConsts.NullCCA)
        return Color.Empty;

      var ccaPalette = (CCAPalette)Palette;

      var ccaValue = cellValue.MeasuredCCA / HALF_PASS_FACTOR;

      if (ccaValue <= ccaPalette.PaletteTransitions.Length - 1)
        return ccaPalette.PaletteTransitions[ccaValue].Color;

      return ccaValue >= CellPassConsts.THICK_LIFT_CCA_VALUE / HALF_PASS_FACTOR ? Color.Empty : ccaPalette.PaletteTransitions[ccaPalette.PaletteTransitions.Length - 1].Color;
    }
  }
}
