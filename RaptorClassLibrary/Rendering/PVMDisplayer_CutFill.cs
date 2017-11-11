using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.Rendering
{
    /// <summary>
    /// Plan View Map displayer renderer for machine speed information presented as rendered tiles
    /// </summary>
    public class PVMDisplayer_CutFill : PVMDisplayerBase
    {
        private ClientHeightLeafSubGrid SubGrid = null;

        protected override bool DoRenderSubGrid(ISubGrid subGrid)
        {
            SubGrid = (subGrid as ClientHeightLeafSubGrid);

            return SubGrid == null ? false : base.DoRenderSubGrid(SubGrid);
        }

        protected override bool SupportsCellStripRendering() => true;

        protected override Color DoGetDisplayColour()
        {
            float value = SubGrid.Cells[east_col, north_row];

            return value == CellPass.NullHeight ? Color.Empty : Palette.ChooseColour(value);
        }
    }
}
 