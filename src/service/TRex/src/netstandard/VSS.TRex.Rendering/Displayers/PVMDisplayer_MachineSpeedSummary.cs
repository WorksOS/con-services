using System.Drawing;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for machine speed summary information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_MachineSpeedSummary : PVMDisplayerBase
  {
    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      if (!(SubGrid is ClientMachineTargetSpeedLeafSubGrid))
        ThrowTRexClientLeafSubGridException();

      var value = ((ClientMachineTargetSpeedLeafSubGrid)SubGrid).Cells[east_col, north_row];

      if (!(Palette is SpeedSummaryPalette))
        ThrowTRexColorPaletteException();

      return value.Max == CellPassConsts.NullMachineSpeed ? Color.Empty : ((SpeedSummaryPalette) Palette).ChooseColour(value);
    }
  }
}
