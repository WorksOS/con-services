using System.Drawing;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Rendering.Palettes.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for MDP information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_MDP : PVMDisplayerBase
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
        CastRequestObjectTo<MDPPalette>(Palette, ThrowTRexColorPaletteTypeCastException<MDPPalette>);
    }

    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      var cellValue = ((ClientMDPLeafSubGrid)SubGrid).Cells[east_col, north_row];

      return cellValue.MeasuredMDP == CellPassConsts.NullMDP ? Color.Empty : ((MDPPalette)Palette).ChooseColour(cellValue.MeasuredMDP, cellValue.TargetMDP);
    }
  }
}
