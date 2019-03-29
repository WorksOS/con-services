using System.Drawing;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for MDP information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_MDPSummary : PVMDisplayerBase
  {
    /// <summary>
    /// Renders MDP summary data as tiles. 
    /// </summary>
    /// <param name="subGrid"></param>
    /// <returns></returns>
    protected override bool DoRenderSubGrid<T>(ISubGrid subGrid)
    {
      return base.DoRenderSubGrid<ClientMDPLeafSubGrid>(subGrid);
    }

    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      var cellValue = ((ClientMDPLeafSubGrid)SubGrid).Cells[east_col, north_row];

      return cellValue.MeasuredMDP == CellPassConsts.NullMDP ? Color.Empty : ((MDPSummaryPalette)Palette).ChooseColour(cellValue.MeasuredMDP, cellValue.TargetMDP);
    }
  }
}
