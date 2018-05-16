using System.Linq;
using log4net;
using System.Reflection;
using VSS.TRex.Executors.Tasks;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering.Executors.Tasks
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
        public PlanViewTileRenderer TileRenderer { get; set; }

        /// <summary>
        /// Constructs the PVM renderer as well as an argument and request to be used if needing to request elevations to support cut/fill operations
        /// </summary>
        /// <param name="requestDescriptor"></param>
        /// <param name="tRexNodeId"></param>
        /// <param name="gridDataType"></param>
        /// <param name="tileRenderer"></param>
        public PVMRenderingTask(long requestDescriptor, 
                                string tRexNodeId, 
                                GridDataType gridDataType, 
                                PlanViewTileRenderer tileRenderer) : base(requestDescriptor, tRexNodeId, gridDataType)
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
        public override bool TransferResponses(object [] responses) => responses.All(TransferResponse);
    }
}
