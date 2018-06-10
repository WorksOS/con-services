using System.Drawing;
using VSS.TRex.Cells;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  /// <summary>
  /// Plan View Map displayer renderer for MDP information presented as rendered tiles
  /// </summary>
  public class PVMDisplayer_MDP : PVMDisplayerBase
  {
    private ClientMDPLeafSubGrid SubGrid;

    protected override bool DoRenderSubGrid(ISubGrid subGrid)
    {
      if (subGrid is ClientMDPLeafSubGrid grid)
      {
        SubGrid = grid;
        return base.DoRenderSubGrid(SubGrid);
      }

      return false;
    }

    protected override bool SupportsCellStripRendering() => true;

    protected override Color DoGetDisplayColour()
    {
      short value = SubGrid.Cells[east_col, north_row];

      return value == CellPass.NullMDP ? Color.Empty : Palette.ChooseColour(value);
    }
  }
}
