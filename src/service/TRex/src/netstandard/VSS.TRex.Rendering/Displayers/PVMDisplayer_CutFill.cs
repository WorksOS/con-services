using System.Drawing;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map  renderer for cut fill information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_CutFill : PVMDisplayerBase
  {
    protected override void SetSubGrid(ISubGrid value)
    {
      base.SetSubGrid(value);

      if (SubGrid != null)
        CastRequestObjectTo<ClientHeightLeafSubGrid>(SubGrid, ThrowTRexClientLeafSubGridTypeCastException<ClientHeightLeafSubGrid>);
    }

    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      float value = ((ClientHeightLeafSubGrid)SubGrid).Cells[east_col, north_row];

      return value == CellPassConsts.NullHeight ? Color.Empty : Palette.ChooseColour(value);
    }
  }
}
