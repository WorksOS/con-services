using System;
using Microsoft.Extensions.Logging;
using System.Reflection;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering.Executors.Tasks
{
  /// <summary>
  /// A Task specialized towards rendering subgrid based information onto Plan View Map tiles
  /// </summary>
  public class PVMRenderingTask : PipelinedSubGridTask, IPVMRenderingTask
  {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// The tile renderer responsible for processing subgrid information into tile based thematic rendering
        /// </summary>
        public PlanViewTileRenderer TileRenderer { get; set; }

        public PVMRenderingTask()
        { }

        /// <summary>
        /// Constructs the PVM rendering task to accept subgrids returning from the processing engine
        /// </summary>
        /// <param name="requestDescriptor"></param>
        /// <param name="tRexNodeId"></param>
        /// <param name="gridDataType"></param>
        /// <param name="tileRenderer"></param>
        public PVMRenderingTask(Guid requestDescriptor, 
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
                Log.LogWarning("Base TransferResponse returned false");
                return false;
            }

            if (!(response is IClientLeafSubGrid[] subGridResponses) || subGridResponses.Length == 0)
              {
                Log.LogWarning("No subgrid responses returned");
                return false;
              }

            foreach (var subGrid in subGridResponses)
            {
              if (subGrid == null)
                continue;

              if (!TileRenderer.Displayer.RenderSubGrid(subGrid))
                return false;
            }

            return true;
        }
    }
}
