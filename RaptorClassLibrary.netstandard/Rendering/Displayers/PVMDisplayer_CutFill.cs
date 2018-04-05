using System.Drawing;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.Rendering.Displayers
{
    /// <summary>
    /// Plan View Map displayer renderer for machine speed information presented as rendered tiles
    /// </summary>
    public class PVMDisplayer_CutFill : PVMDisplayerBase
    {
        private ClientHeightLeafSubGrid SubGrid;

        protected override bool DoRenderSubGrid(ISubGrid subGrid)
        {
            SubGrid = (subGrid as ClientHeightLeafSubGrid);

            return SubGrid != null && base.DoRenderSubGrid(SubGrid);
        }

        protected override bool SupportsCellStripRendering() => true;

        protected override Color DoGetDisplayColour()
        {
            float value = SubGrid.Cells[east_col, north_row];

            return value == CellPass.NullHeight ? Color.Empty : Palette.ChooseColour(value);
        }
    }
}
 