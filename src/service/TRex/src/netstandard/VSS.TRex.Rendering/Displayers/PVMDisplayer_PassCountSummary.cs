using System.Drawing;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Records;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for Pass Count summary information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_PassCountSummary : PVMDisplayerBase
  {
    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      if (!(SubGrid is ClientPassCountLeafSubGrid))
        ThrowTRexClientLeafSubGridException();

      var cellValue = ((ClientPassCountLeafSubGrid)SubGrid).Cells[east_col, north_row];

      if (!(Palette is PassCountSummaryPalette))
        ThrowTRexColorPaletteException();

      return cellValue.MeasuredPassCount == CellPassConsts.NullPassCountValue ? Color.Empty : ((PassCountSummaryPalette)Palette).ChooseColour(cellValue.MeasuredPassCount, cellValue.TargetPassCount, cellValue.TargetPassCount);
    }
  }
}
