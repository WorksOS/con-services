using System.Drawing;
using VSS.TRex.Cells;
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

    private Color ChooseColour(MachineSpeedExtendedRecord measuredSpeed, MachineSpeedExtendedRecord targetSpeed)
    {
      if (targetSpeed.Max == CellPass.NullMachineSpeed)
        return Color.Empty;
      else
      {
        if (measuredSpeed.Max > targetSpeed.Max)
          return Color.Purple;
        else if (measuredSpeed.Min < targetSpeed.Min && measuredSpeed.Max < targetSpeed.Min)
          return Color.Lime;
        else
          return Color.Aqua;
      }
    }

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

      return value.Max == CellPass.NullMachineSpeed ? Color.Empty : ChooseColour(value, machineSpeedTarget);
    }

  }
}
