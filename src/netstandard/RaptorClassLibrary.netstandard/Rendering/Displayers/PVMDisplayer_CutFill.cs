using System.Drawing;
using VSS.TRex.Cells;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for machine speed information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_CutFill : PVMDisplayerBase
  {
    /// <summary>
    /// Cut/Fill data holder.
    private ClientHeightLeafSubGrid SubGrid;

    /// <summary>
    /// Renders Cut/Fill summary data as tiles. 
    /// </summary>
    /// <param name="subGrid"></param>
    /// <returns></returns>
    protected override bool DoRenderSubGrid(ISubGrid subGrid)
    {
      SubGrid = (subGrid as ClientHeightLeafSubGrid);

      return SubGrid != null && base.DoRenderSubGrid(SubGrid);
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
      float value = SubGrid.Cells[east_col, north_row];

      return value == CellPass.NullHeight ? Color.Empty : Palette.ChooseColour(value);
    }
  }
}
