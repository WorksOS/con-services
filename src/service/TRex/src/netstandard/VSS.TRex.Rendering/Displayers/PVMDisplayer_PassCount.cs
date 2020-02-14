using System.Drawing;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for Pass Count information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_PassCount : PVMDisplayerBase<PassCountPalette, ClientPassCountLeafSubGrid, SubGridCellPassDataPassCountEntryRecord>
  {
    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    public override Color DoGetDisplayColour()
    {
      var value = ValueStore[east_col, north_row];

      return value.MeasuredPassCount == CellPassConsts.NullPassCountValue ? Color.Empty : Palette.ChooseColour(value.MeasuredPassCount);
    }
  }
}
