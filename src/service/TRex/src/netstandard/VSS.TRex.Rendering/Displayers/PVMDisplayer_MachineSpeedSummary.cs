using System.Drawing;
using VSS.TRex.Common.Records;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for machine speed summary information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_MachineSpeedSummary : PVMDisplayerBase<SpeedSummaryPalette, ClientMachineTargetSpeedLeafSubGrid, MachineSpeedExtendedRecord>
  {
    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    public override Color DoGetDisplayColour()
    {
      var value = ValueStore[east_col, north_row];

      return value.Max == CellPassConsts.NullMachineSpeed ? Color.Empty : Palette.ChooseColour(value);
    }
  }
}
