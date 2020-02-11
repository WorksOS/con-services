using System.Drawing;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for Pass Count summary information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_PassCountSummary : PVMDisplayerBase<PassCountSummaryPalette, ClientPassCountLeafSubGrid>
  {
    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    public override Color DoGetDisplayColour()
    {
      var cellValue = SubGrid.Cells[east_col, north_row];

      return cellValue.MeasuredPassCount == CellPassConsts.NullPassCountValue ? Color.Empty : Palette.ChooseColour(cellValue.MeasuredPassCount, cellValue.TargetPassCount, cellValue.TargetPassCount);
    }
  }
}
