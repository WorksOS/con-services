using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using System.Drawing;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for CCA information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_CCA : PVMDisplayerBase
  {
    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      var cellValue = ((ClientCCALeafSubGrid)SubGrid).Cells[east_col, north_row];

      return cellValue.MeasuredCCA == CellPassConsts.NullCCA || cellValue.TargetCCA == CellPassConsts.NullCCATarget ? Color.Empty : ((CCASummaryPalette)Palette).ChooseColour(cellValue);
    }
  }
}
