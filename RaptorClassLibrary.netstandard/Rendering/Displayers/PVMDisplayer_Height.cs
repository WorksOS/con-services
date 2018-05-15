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
        private ClientHeightLeafSubGrid SubGrid;

        protected override bool DoRenderSubGrid(ISubGrid subGrid)
        {
            if (subGrid is ClientHeightLeafSubGrid grid)
            {
                SubGrid = grid;
                return base.DoRenderSubGrid(SubGrid);
            }

            return false;
        }

        protected override bool SupportsCellStripRendering() => true;

        protected override Color DoGetDisplayColour()
        {
            float Height = SubGrid.Cells[east_col, north_row];

            return Height == Consts.NullHeight ? Color.Empty : Palette.ChooseColour(Height);
        }
    }
}
 