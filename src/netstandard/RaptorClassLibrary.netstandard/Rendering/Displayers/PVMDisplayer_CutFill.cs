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
 