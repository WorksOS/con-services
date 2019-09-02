using System.Drawing;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Rendering.Palettes.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for machine speed summary information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_MachineSpeedSummary : PVMDisplayerBase
  {
    protected override void SetSubGrid(ISubGrid value)
    {
      base.SetSubGrid(value);

      if (SubGrid != null)
        CastRequestObjectTo<ClientMachineTargetSpeedLeafSubGrid>(SubGrid, ThrowTRexClientLeafSubGridTypeCastException<ClientMachineTargetSpeedLeafSubGrid>);
    }

    protected override void SetPalette(IPlanViewPalette value)
    {
      base.SetPalette(value);

      if (Palette != null)
        CastRequestObjectTo<SpeedSummaryPalette>(Palette, ThrowTRexColorPaletteTypeCastException<SpeedSummaryPalette>);
    }

    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      var value = ((ClientMachineTargetSpeedLeafSubGrid)SubGrid).Cells[east_col, north_row];

      return value.Max == CellPassConsts.NullMachineSpeed ? Color.Empty : ((SpeedSummaryPalette) Palette).ChooseColour(value);
    }
  }
}
