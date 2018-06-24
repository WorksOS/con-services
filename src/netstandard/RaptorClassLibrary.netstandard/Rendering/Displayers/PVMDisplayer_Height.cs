using System.Drawing;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for height/elevation information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_Height : PVMDisplayerBase
  {
    /// <summary>
    /// Elevation data holder. 
    /// </summary>
    private ClientHeightLeafSubGrid SubGrid;

    /// <summary>
    /// Renders Elevationy data as tiles. 
    /// </summary>
    /// <param name="subGrid"></param>
    /// <returns></returns>
    protected override bool DoRenderSubGrid(ISubGrid subGrid)
    {
      if (subGrid is ClientHeightLeafSubGrid grid)
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
      float Height = SubGrid.Cells[east_col, north_row];

      return Height == Consts.NullHeight ? Color.Empty : Palette.ChooseColour(Height);
    }
  }
}
