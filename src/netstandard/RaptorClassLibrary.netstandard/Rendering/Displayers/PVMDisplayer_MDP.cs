using System.Drawing;
using VSS.TRex.Cells;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for MDP information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_MDP : PVMDisplayerBase
  {
    private const bool UseMachineTargetMDP = false;
    private const short AbsoluteTargetMDP = 50;

    private ClientMDPLeafSubGrid SubGrid;

    protected override bool DoRenderSubGrid(ISubGrid subGrid)
    {
      if (subGrid is ClientMDPLeafSubGrid grid)
      {
        SubGrid = grid;
        return base.DoRenderSubGrid(SubGrid);
      }

      return false;
    }

    protected override bool SupportsCellStripRendering() => true;

    protected override Color DoGetDisplayColour()
    {
      var subGrid = SubGrid.Cells[east_col, north_row];

      if (subGrid.MeasuredMDP == CellPass.NullMDP)
        return Color.Empty;

      var targetMDPValue = subGrid.TargetMDP;

      // If we are not using the machine target MDP value then we need to replace the
      // target MDP report from the machine, with the override value specified in the options
      if (!UseMachineTargetMDP)
        targetMDPValue = AbsoluteTargetMDP;

      return ((MDPPalette)Palette).ChooseColour(subGrid.MeasuredMDP, targetMDPValue);
    }
  }
}
