using Draw = System.Drawing;
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
    /// Renders Elevationy data as tiles. 
    /// </summary>
    /// <param name="subGrid"></param>
    /// <returns></returns>
    protected override bool DoRenderSubGrid<T>(ISubGrid subGrid)
    {
      return base.DoRenderSubGrid<ClientHeightLeafSubGrid>(subGrid);
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
    protected override Draw.Color DoGetDisplayColour()
    {
      float Height = ((ClientHeightLeafSubGrid)SubGrid).Cells[east_col, north_row];

      return Height == Consts.NullHeight ? Draw.Color.Empty : Palette.ChooseColour(Height);
    }
  }
}
