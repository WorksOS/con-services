using System.Drawing;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Types;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for CCA summary information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_CCASummary : PVMDisplayerBase<CCASummaryPalette, ClientCCALeafSubGrid, SubGridCellPassDataCCAEntryRecord>
  {
    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    public override Color DoGetDisplayColour()
    {
      var cellValue = ValueStore[east_col, north_row];

      return cellValue.MeasuredCCA == CellPassConsts.NullCCA || cellValue.TargetCCA == CellPassConsts.NullCCATarget ? Color.Empty : Palette.ChooseColour(cellValue);
    }
  }
}
