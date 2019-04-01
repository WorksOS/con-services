using VSS.TRex.Common.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using System.Drawing;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for machine speed information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_CMV : PVMDisplayerBase
  {
    /// <summary>
    /// The default colour that is used to display decoupled CMV data.
    /// </summary>
    private Color DefaultDecoupledCMVColour = Color.Black;

    /// <summary>
    /// The flag is to indicate whether or not the machine CMV target to be user overrides.
    /// </summary>
    private const bool UseMachineTargetCMV = false;

    /// <summary>
    /// Default overriding CMV target value.
    /// </summary>
    private const short AbsoluteTargetCMV = 70;

    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      var cellValue = ((ClientCMVLeafSubGrid)SubGrid).Cells[east_col, north_row];

      if (cellValue.MeasuredCMV == CellPassConsts.NullCCV)
        return Color.Empty;

      var decoupled = cellValue.IsDecoupled && ((CMVPalette) Palette).DisplayDecoupledColourInPVM;

      if (decoupled)
        return DefaultDecoupledCMVColour;

      var targetCMVValue = cellValue.TargetCMV;

      // If we are not using the machine target CCV value then we need to replace the
      // target CMV report from the machine, with the override value specified here.
      if (!UseMachineTargetCMV)
        targetCMVValue = AbsoluteTargetCMV;
        
      return ((CMVPalette) Palette).ChooseColour(cellValue.MeasuredCMV, targetCMVValue);
    }
  }
}
