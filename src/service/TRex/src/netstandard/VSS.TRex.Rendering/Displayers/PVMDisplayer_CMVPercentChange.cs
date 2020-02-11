using System.Drawing;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;

namespace VSS.TRex.Rendering.Displayers
{
  public class PVMDisplayer_CMVPercentChange : PVMDisplayerBase<CMVPercentChangePalette, ClientCMVLeafSubGrid>
  {
    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    public override Color DoGetDisplayColour()
    {
      var cellValue = SubGrid.Cells[east_col, north_row];

      return cellValue.MeasuredCMV == CellPassConsts.NullCCV ? Color.Empty : Palette.ChooseColour(cellValue);
    }
  }
}
