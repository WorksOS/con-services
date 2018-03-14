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

namespace VSS.VisionLink.Raptor.Rendering.Displayers
{
    /// <summary>
    /// Plan View Map displayer renderer for machine speed information presented as rendered tiles
    /// </summary>
    public class PVMDisplayer_Temperature : PVMDisplayerBase
    {
        private ClientTemperatureLeafSubGrid SubGrid = null;

        protected override bool DoRenderSubGrid(ISubGrid subGrid)
        {
            if (subGrid is ClientTemperatureLeafSubGrid)
            {
                SubGrid = (ClientTemperatureLeafSubGrid)subGrid;
                return base.DoRenderSubGrid(SubGrid);
            }

            return false;
        }

        protected override bool SupportsCellStripRendering() => true;

        protected override Color DoGetDisplayColour()
        {
            ushort value = SubGrid.Cells[east_col, north_row];

            return value == CellPass.NullMaterialTemp ? Color.Empty : Palette.ChooseColour(value);
        }
    }
}
 