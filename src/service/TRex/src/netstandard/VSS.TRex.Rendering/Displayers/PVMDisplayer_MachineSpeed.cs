using System.Drawing;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.SubGridTrees.Client;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for machine speed information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_MachineSpeed : PVMDisplayerBase<SpeedPalette, ClientMachineSpeedLeafSubGrid>
  {
    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    public override Color DoGetDisplayColour()
    {
      var value = SubGrid.Cells[east_col, north_row];

      return value == CellPassConsts.NullMachineSpeed ? Color.Empty : Palette.ChooseColour(value);
    }
  }
}
