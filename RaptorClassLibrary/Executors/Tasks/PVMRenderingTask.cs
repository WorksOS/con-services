using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Rendering;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Executors.Tasks
{
    /// <summary>
    /// A Task specialised towards rendering subgrid based information onto Plan View Map tiles
    /// </summary>
    public class PVMRenderingTask : PipelinedSubGridTask
    {
        /// <summary>
        /// The tile renderer responsible for processing subgrid information into tile based thematic rendering
        /// </summary>
        public PlanViewTileRenderer TileRenderer { get; set; } = null;

        public PVMRenderingTask(long requestDescriptor, GridDataType gridDataType) : base(requestDescriptor, gridDataType)
        {
        }

        public PVMRenderingTask(long requestDescriptor, GridDataType gridDataType, PlanViewTileRenderer tileRenderer) : this(requestDescriptor, gridDataType)
        {
            TileRenderer = tileRenderer;
        }

        public override bool TransferResponse(object response)
        {
            if (!base.TransferResponse(response))
            {
                return false;
            }

            return TileRenderer.Displayer.RenderSubGrid(response as IClientLeafSubGrid);
        }
    }
}
