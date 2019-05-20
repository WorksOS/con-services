using System.Drawing;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for CCA summary information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_CCASummary : PVMDisplayerBase
  {
    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      if (!(SubGrid is ClientCCALeafSubGrid))
        ThrowTRexClientLeafSubGridException();

      var cellValue = ((ClientCCALeafSubGrid)SubGrid).Cells[east_col, north_row];

      if (!(Palette is CCASummaryPalette))
        ThrowTRexColorPaletteException();

      return cellValue.MeasuredCCA == CellPassConsts.NullCCA || cellValue.TargetCCA == CellPassConsts.NullCCATarget ? Color.Empty : ((CCASummaryPalette)Palette).ChooseColour(cellValue);
    }
  }
}
