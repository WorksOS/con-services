using System.Drawing;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Types;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for MDP information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_MDP : PVMDisplayerBase<MDPPalette, ClientMDPLeafSubGrid, SubGridCellPassDataMDPEntryRecord>
  {
    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    public override Color DoGetDisplayColour()
    {
      var cellValue = ValueStore[east_col, north_row];

      return cellValue.MeasuredMDP == CellPassConsts.NullMDP ? Color.Empty : Palette.ChooseColour(cellValue.MeasuredMDP, cellValue.TargetMDP);
    }
  }
}
