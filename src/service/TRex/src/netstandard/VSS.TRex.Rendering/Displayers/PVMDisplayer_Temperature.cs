using System.Drawing;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for material temperature information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_Temperature : PVMDisplayerBase
  {
    protected override void SetSubGrid(ISubGrid value)
    {
      base.SetSubGrid(value);

      if (SubGrid != null)
        CastRequestObjectTo<ClientTemperatureLeafSubGrid>(SubGrid, ThrowTRexClientLeafSubGridTypeCastException<ClientTemperatureLeafSubGrid>);
    }

    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      ushort value = ((ClientTemperatureLeafSubGrid)SubGrid).Cells[east_col, north_row].MeasuredTemperature;

      return value == CellPassConsts.NullMaterialTemperatureValue ? Color.Empty : Palette.ChooseColour(value);
    }
  }
}
