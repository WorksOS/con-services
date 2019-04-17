using System.Drawing;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for CMV information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_CMV : PVMDisplayerBase
  {
    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      var cellValue = ((ClientCMVLeafSubGrid)SubGrid).Cells[east_col, north_row];

      return cellValue.MeasuredCMV == CellPassConsts.NullCCV ? Color.Empty : ((CMVPalette) Palette).ChooseColour(cellValue);
    }
  }
}
