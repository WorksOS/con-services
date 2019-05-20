using System.Drawing;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for machine speed information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_MachineSpeed : PVMDisplayerBase
  {
    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      if (!(SubGrid is ClientMachineSpeedLeafSubGrid))
        ThrowTRexClientLeafSubGridException();

      var value = ((ClientMachineSpeedLeafSubGrid)SubGrid).Cells[east_col, north_row];

      return value == CellPassConsts.NullMachineSpeed ? Color.Empty : Palette.ChooseColour(value);
    }
  }
}
