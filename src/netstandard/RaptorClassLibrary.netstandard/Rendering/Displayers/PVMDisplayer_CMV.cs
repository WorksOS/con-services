using System.Drawing;
using VSS.TRex.Cells;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
    /// <summary>
    /// Plan View Map displayer renderer for machine speed information presented as rendered tiles
    /// </summary>
    public class PVMDisplayer_CMV : PVMDisplayerBase
    {
        private ClientCMVLeafSubGrid SubGrid;

        protected override bool DoRenderSubGrid(ISubGrid subGrid)
        {
            if (subGrid is ClientCMVLeafSubGrid grid)
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

            return value == CellPass.NullCCV ? Color.Empty : Palette.ChooseColour(value);
        }
    }
}
 