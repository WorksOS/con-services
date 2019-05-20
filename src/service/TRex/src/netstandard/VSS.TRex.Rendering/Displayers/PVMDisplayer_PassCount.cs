using System.Drawing;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for Pass Count information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_PassCount : PVMDisplayerBase
  {
    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      if (!(SubGrid is ClientPassCountLeafSubGrid))
        ThrowTRexClientLeafSubGridException();

      var value = ((ClientPassCountLeafSubGrid)SubGrid).Cells[east_col, north_row];

      return value.MeasuredPassCount == CellPassConsts.NullPassCountValue ? Color.Empty : Palette.ChooseColour(value.MeasuredPassCount);
    }
  }
}
