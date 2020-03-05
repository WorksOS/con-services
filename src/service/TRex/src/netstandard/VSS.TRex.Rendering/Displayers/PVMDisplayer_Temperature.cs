using System.Drawing;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Types;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for material temperature information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_Temperature : PVMDisplayerBase<TemperaturePalette, ClientTemperatureLeafSubGrid, SubGridCellPassDataTemperatureEntryRecord>
  {
    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    public override Color DoGetDisplayColour()
    {
      ushort value = ValueStore[east_col, north_row].MeasuredTemperature;

      return value == CellPassConsts.NullMaterialTemperatureValue ? Color.Empty : Palette.ChooseColour(value);
    }
  }
}
