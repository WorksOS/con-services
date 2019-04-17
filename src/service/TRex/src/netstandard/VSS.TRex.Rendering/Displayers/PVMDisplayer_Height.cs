using System.Drawing;
using VSS.TRex.Common;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for height/elevation information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_Height : PVMDisplayerBase
  {
    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      float Height = ((ClientHeightLeafSubGrid)SubGrid).Cells[east_col, north_row];

      return Height == Consts.NullHeight ? Color.Empty : ((HeightPalette)Palette).ChooseColour(Height);
    }
  }
}
