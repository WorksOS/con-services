using Draw = System.Drawing;
using VSS.TRex.Common.CellPasses;
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
    /// <summary>
    /// Machine Speed target range.
    /// </summary>
    public MachineSpeedExtendedRecord machineSpeedTarget;

    /// <summary>
    /// Renders Machine Speed summary data as tiles. 
    /// </summary>
    /// <param name="subGrid"></param>
    /// <returns></returns>
    protected override bool DoRenderSubGrid<T>(ISubGrid subGrid)
    {
      return base.DoRenderSubGrid<ClientMachineTargetSpeedLeafSubGrid>(subGrid);
    }

    /// <summary>
    ///  Enables a displayer to advertise is it capable of rendering cell information in strips.
    /// </summary>
    /// <returns></returns>
    protected override bool SupportsCellStripRendering() => true;

    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Draw.Color DoGetDisplayColour()
    {
      var value = ((ClientMachineTargetSpeedLeafSubGrid)SubGrid).Cells[east_col, north_row];

      return value.Max == CellPassConsts.NullMachineSpeed ? Draw.Color.Empty : ((SpeedSummaryPalette) Palette).ChooseColour(value, machineSpeedTarget);
    }

  }
}
