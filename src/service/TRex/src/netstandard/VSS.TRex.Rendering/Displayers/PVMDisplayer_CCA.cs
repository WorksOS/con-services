using System.Drawing;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Types;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for CCA information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_CCA : PVMDisplayerBase<CCAPalette, ClientCCALeafSubGrid, SubGridCellPassDataCCAEntryRecord>
  {
    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
   public override Color DoGetDisplayColour()
    {
      const byte HALF_PASS_FACTOR = 2;

      var cellValue = ValueStore[east_col, north_row];

      if (cellValue.MeasuredCCA == CellPassConsts.NullCCA)
        return Color.Empty;

      var ccaPalette = Palette;

      var ccaValue = cellValue.MeasuredCCA / HALF_PASS_FACTOR;

      if (ccaValue <= ccaPalette.PaletteTransitions.Length - 1)
        return ccaPalette.PaletteTransitions[ccaValue].Color;

      return ccaValue >= CellPassConsts.THICK_LIFT_CCA_VALUE / HALF_PASS_FACTOR ? Color.Empty : ccaPalette.PaletteTransitions[ccaPalette.PaletteTransitions.Length - 1].Color;
    }
  }
}
