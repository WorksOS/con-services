using System.Drawing;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.Rendering.Displayers
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
 