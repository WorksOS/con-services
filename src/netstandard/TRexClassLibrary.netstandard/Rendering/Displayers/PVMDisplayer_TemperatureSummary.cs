using System.Drawing;
using VSS.TRex.Cells;
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
    /// Temperature data holder.
    /// </summary>
    private ClientTemperatureLeafSubGrid SubGrid;
      
    /// <summary>
    /// Renders Temperature summary data as tiles. 
    /// </summary>
    /// <param name="subGrid"></param>
    /// <returns></returns>
    protected override bool DoRenderSubGrid(ISubGrid subGrid)
    {
      if (subGrid is ClientTemperatureLeafSubGrid grid)
      {
          SubGrid = grid;
          return base.DoRenderSubGrid(SubGrid);
      }

      return false;
    }

    /// <summary>
    ///  Enables a displayer to advertise is it capable of rendering cell information in strips.
    /// </summary>
    /// <returns></returns>
    protected override bool SupportsCellStripRendering() => true;

    /// <summary>
    /// Queries the data at the current cell location and determines the colour that should be displayed there.
    /// </summary>
    /// <returns></returns>
    protected override Color DoGetDisplayColour()
    {
      ushort value = SubGrid.Cells[east_col, north_row].MeasuredTemperature;

      return value == CellPass.NullMaterialTemperatureValue ? Color.Empty : Palette.ChooseColour(value);
    }
  }
}
