using System.Drawing;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Rendering.Palettes.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for MDP information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_MDPSummary : PVMDisplayerBase
  {
    protected override void SetSubGrid(ISubGrid value)
    {
      base.SetSubGrid(value);

      if (SubGrid != null)
        CastRequestObjectTo<ClientMDPLeafSubGrid>(SubGrid, ThrowTRexClientLeafSubGridTypeCastException<ClientMDPLeafSubGrid>);
    }

    protected override void SetPalette(IPlanViewPalette value)
    {
      base.SetPalette(value);

      if (Palette != null)
        CastRequestObjectTo<MDPSummaryPalette>(Palette, ThrowTRexColorPaletteTypeCastException<MDPSummaryPalette>);
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
