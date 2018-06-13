using System.Drawing;
using VSS.TRex.Cells;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for machine speed summary information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_MachineSpeedSummary : PVMDisplayerBase
  {
    private ClientMachineTargetSpeedLeafSubGrid SubGrid;

    public MachineSpeedExtendedRecord machineSpeedTarget;

    protected override bool DoRenderSubGrid(ISubGrid subGrid)
    {
      if (subGrid is ClientMachineTargetSpeedLeafSubGrid grid)
      {
        SubGrid = grid;
        return base.DoRenderSubGrid(SubGrid);
      }

      return false;
    }

    protected override bool SupportsCellStripRendering() => true;

    protected override Color DoGetDisplayColour()
    {
      var value = SubGrid.Cells[east_col, north_row];

      return value.Max == CellPass.NullMachineSpeed ? Color.Empty : ((SpeedSummaryPalette) Palette).ChooseColour(value, machineSpeedTarget);
    }

  }
}
