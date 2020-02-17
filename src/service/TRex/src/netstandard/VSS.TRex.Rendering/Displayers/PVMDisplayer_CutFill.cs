using System.Drawing;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.SubGridTrees.Client;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map  renderer for cut fill information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_CutFill : PVMDisplayerBase<CutFillPalette, ClientHeightLeafSubGrid, float>
  {
    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    public override Color DoGetDisplayColour()
    {
      var value = ValueStore[east_col, north_row];

      return value == CellPassConsts.NullHeight ? Color.Empty : Palette.ChooseColour(value);
    }
  }
}
