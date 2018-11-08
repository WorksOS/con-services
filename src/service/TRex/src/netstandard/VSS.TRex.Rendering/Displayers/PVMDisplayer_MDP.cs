using Draw = System.Drawing;
using VSS.TRex.Common.CellPasses;
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
    /// <summary>
    /// The flag is to indicate wehther or not the machine MDP target to be user overrides.
    /// </summary>
    private const bool UseMachineTargetMDP = false;

    /// <summary>
    /// Default overriding MDP target value.
    /// </summary>
    private const short AbsoluteTargetMDP = 50;

    /// <summary>
    /// MDP data holder.
    /// </summary>
    private ClientMDPLeafSubGrid SubGrid;

    /// <summary>
    /// Renders MDP summary data as tiles. 
    /// </summary>
    /// <param name="subGrid"></param>
    /// <returns></returns>
    protected override bool DoRenderSubGrid(ISubGrid subGrid)
    {
      if (subGrid is ClientMDPLeafSubGrid grid)
      {
        SubGrid = grid;
        return base.DoRenderSubGrid(SubGrid);
      }

      return false;
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
      var cellValue = SubGrid.Cells[east_col, north_row];

      if (cellValue.MeasuredMDP == CellPassConsts.NullMDP)
        return Draw.Color.Empty;

      var targetMDPValue = cellValue.TargetMDP;

      // If we are not using the machine target MDP value then we need to replace the
      // target MDP report from the machine, with the override value specified in the options
      if (!UseMachineTargetMDP)
        targetMDPValue = AbsoluteTargetMDP;

      return ((MDPPalette)Palette).ChooseColour(cellValue.MeasuredMDP, targetMDPValue);
    }
  }
}
