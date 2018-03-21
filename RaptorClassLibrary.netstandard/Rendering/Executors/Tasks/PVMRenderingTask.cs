using log4net;
using System.Reflection;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Executors.Tasks;
using System;

namespace VSS.VisionLink.Raptor.Rendering.Executors.Tasks
{
    /// <summary>
    /// A Task specialised towards rendering subgrid based information onto Plan View Map tiles
    /// </summary>
    public class PVMRenderingTask : PipelinedSubGridTask
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The tile renderer responsible for processing subgrid information into tile based thematic rendering
        /// </summary>
        public PlanViewTileRenderer TileRenderer { get; set; } = null;

        /// <summary>
        /// Constructs the PVM renderer as well as an argument and request to be used if needing to request elevations to support cut/fill operations
        /// </summary>
        /// <param name="requestDescriptor"></param>
        /// <param name="raptorNodeID"></param>
        /// <param name="gridDataType"></param>
        /// <param name="tileRenderer"></param>
        /// <param name="cutFillDesignID"></param>
        public PVMRenderingTask(long requestDescriptor, 
                                string raptorNodeID, 
                                GridDataType gridDataType, 
                                PlanViewTileRenderer tileRenderer) : base(requestDescriptor, raptorNodeID, gridDataType)
        {
            TileRenderer = tileRenderer;
        }

        public override bool TransferResponse(object response)
        {
            // Log.InfoFormat("Received a SubGrid to be processed: {0}", (response as IClientLeafSubGrid).Moniker());

            if (!base.TransferResponse(response))
            {
                Log.Warn("Base TransferResponse returned false");
                return false;
            }

            return TileRenderer.Displayer.RenderSubGrid((response as IClientLeafSubGrid[])[0]);
        }

        /// <summary>
        /// Transfers a set of subgrids responses into the processing task
        /// </summary>
        /// <param name="responses"></param>
        /// <returns></returns>
        public override bool TransferResponses(object [] responses)
        {
            foreach (Object response in responses)
            {
                if (!TransferResponse(response))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
