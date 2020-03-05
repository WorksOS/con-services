using System.Drawing;
using VSS.TRex.Common;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for height/elevation information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_Height : PVMDisplayerBase<HeightPalette, ClientHeightAndTimeLeafSubGrid, float>
  {
    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    public override Color DoGetDisplayColour()
    {
      var height = ValueStore[east_col, north_row];

      return height == Consts.NullHeight ? Color.Empty : Palette.ChooseColour(height);
    }
  }
}
