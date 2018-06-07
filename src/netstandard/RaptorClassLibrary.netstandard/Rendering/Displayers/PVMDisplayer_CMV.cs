using System.Drawing;
using VSS.TRex.Cells;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for machine speed information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_CMV : PVMDisplayerBase
  {
    private Color DefaultDecoupledCMVColour = Color.Black;
    private const bool UseMachineTargetCMV = false;
    private const short AbsoluteTargetCMV = 70;


    private ClientCMVLeafSubGrid SubGrid;

    protected override bool DoRenderSubGrid(ISubGrid subGrid)
    {
      if (subGrid is ClientCMVLeafSubGrid grid)
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

      if (subGrid.MeasuredCMV == CellPass.NullCCV)
        return Color.Empty;

      var decoupled = subGrid.IsDecoupled && ((CMVPalette) Palette).DisplayDecoupledColourInPVM;

      if (decoupled)
        return DefaultDecoupledCMVColour;
      else
      {
        var targetCMVValue = subGrid.TargetCMV;

        // If we are not using the machine target CCV value then we need to replace the
        // target CMV report from the machine, with the override value specified here.
        if (!UseMachineTargetCMV)
          targetCMVValue = AbsoluteTargetCMV;


        return ((CMVPalette) Palette).ChooseColour(subGrid.MeasuredCMV, targetCMVValue);
      }
    }
  }
}
