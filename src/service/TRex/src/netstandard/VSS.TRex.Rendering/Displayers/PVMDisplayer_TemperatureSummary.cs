using System.Drawing;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for machine speed information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_TemperatureSummary : PVMDisplayerBase
  {
    /// <summary>
    /// Renders Temperature summary data as tiles. 
    /// </summary>
    /// <param name="subGrid"></param>
    /// <returns></returns>
    protected override bool DoRenderSubGrid<T>(ISubGrid subGrid)
    {
      return base.DoRenderSubGrid<ClientTemperatureLeafSubGrid>(subGrid);
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
