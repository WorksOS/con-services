using System.Drawing;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Rendering.Palettes.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for CMV information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_CMV : PVMDisplayerBase
  {
    protected override void SetSubGrid(ISubGrid value)
    {
      base.SetSubGrid(value);

      if (SubGrid != null)
        CastRequestObjectTo<ClientCMVLeafSubGrid>(SubGrid, ThrowTRexClientLeafSubGridTypeCastException<ClientCMVLeafSubGrid>);
    }

    protected override void SetPalette(IPlanViewPalette value)
    {
      base.SetPalette(value);

      if (Palette != null)
        CastRequestObjectTo<CMVPalette>(Palette, ThrowTRexColorPaletteTypeCastException<CMVPalette>);
    }

    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      var cellValue = ((ClientCMVLeafSubGrid)SubGrid).Cells[east_col, north_row];

      return cellValue.MeasuredCMV == CellPassConsts.NullCCV ? Color.Empty : ((CMVPalette) Palette).ChooseColour(cellValue);
    }
  }
}
