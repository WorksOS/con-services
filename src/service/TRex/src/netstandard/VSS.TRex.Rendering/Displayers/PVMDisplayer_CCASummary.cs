using System.Drawing;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Rendering.Palettes.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for CCA summary information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_CCASummary : PVMDisplayerBase
  {
    protected override void SetSubGrid(ISubGrid value)
    {
      base.SetSubGrid(value);

      if (SubGrid != null)
        CastRequestObjectTo<ClientCCALeafSubGrid>(SubGrid, ThrowTRexClientLeafSubGridTypeCastException<ClientCCALeafSubGrid>);
    }

    protected override void SetPalette(IPlanViewPalette value)
    {
      base.SetPalette(value);

      if (Palette != null)
        CastRequestObjectTo<CCASummaryPalette>(Palette, ThrowTRexColorPaletteTypeCastException<CCASummaryPalette>);
    }

    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      var cellValue = ((ClientCCALeafSubGrid)SubGrid).Cells[east_col, north_row];

      return cellValue.MeasuredCCA == CellPassConsts.NullCCA || cellValue.TargetCCA == CellPassConsts.NullCCATarget ? Color.Empty : ((CCASummaryPalette)Palette).ChooseColour(cellValue);
    }
  }
}
