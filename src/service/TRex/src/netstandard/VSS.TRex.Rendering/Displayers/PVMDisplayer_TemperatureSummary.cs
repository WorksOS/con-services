using System.Drawing;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Records;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for material temperature summary information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_TemperatureSummary : PVMDisplayerBase
  {
    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      var cellValue = ((ClientTemperatureLeafSubGrid)SubGrid).Cells[east_col, north_row];
      
      return cellValue.MeasuredTemperature == CellPassConsts.NullMaterialTemperatureValue ? Color.Empty : ((TemperatureSummaryPalette)Palette).ChooseColour(cellValue.MeasuredTemperature, cellValue.TemperatureLevels.Min, cellValue.TemperatureLevels.Max);
    }
  }
}
